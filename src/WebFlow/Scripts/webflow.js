const os = require("os");
const fs = require("fs");
const path = require("path");
const csso = require("csso");
const ts = require("typescript");
const sass = require("node-sass");
const uglify = require("uglify-js");
//const convert = require("convert-source-map");
//const combine = require("combine-source-map");
const mssMerger = require("multi-stage-sourcemap");

var root = {
    // #region OPTIONS
    // ==========================================================================================
    
    TranspilierOptions: function () {
        this.suffix = process.argv[6];
        this.outputFile = process.argv[5];
        this.sourceMapDirectory = process.argv[7];
        this.outputDirectory = (this.outputFile ? path.dirname(process.argv[5]) : false);

        let pattern = /true/i;
        this.generateSourceMaps = (pattern.test(process.argv[3]));
        this.keepIntermediateFiles = (pattern.test(process.argv[2]));
        this.sourceFiles = (process.argv[4] ? process.argv[4].split(';') : "");
        this.bundling = pattern.test(process.argv[8]);
    },

    // #endregion

    // METHODS
    // ==========================================================================================

    compileTs: function (args, onSuccess) {
        let outFile = new root.FileInfo(args.outputFile);

        let options = {};
        options.sourceMap = true;
        options.noEmitOnError = true;
        options.mapRoot = args.sourceMapDirectory;

        if (args.bundling) {
            options["allowJs"] = true;
            options["outFile"] = outFile.path;
        }

        let expectedName = path.basename(outFile.path);
        let compiler = ts.createProgram(args.sourceFiles, options);
        let target = (args.bundling ? null : compiler.getSourceFile(outFile.path));
        let emitResult = compiler.emit(target, function (filePath, contents, bom, onError) {
            if (path.basename(filePath).startsWith(expectedName)) {
                switch (path.extname(filePath).toLowerCase()) {
                    case ".map":
                        let map = JSON.parse(contents);
                        map.file = path.relative(args.sourceMapDirectory, outFile.path);
                        outFile.sourceMap = JSON.stringify(map, null, 2);
                        if (args.generateSourceMaps && args.keepIntermediateFiles) {
                            root.createFile(path.join(args.sourceMapDirectory, path.basename(filePath)), outFile.sourceMap, "sourceMapFile2");
                        }
                        break;

                    case ".js":
                        let mapPath = path.join(args.sourceMapDirectory, (path.basename(filePath) + ".map"));
                        outFile.contents = root.changeSourceMapUrl(contents, path.relative(args.outputDirectory, mapPath));
                        outFile.path = filePath;

                        if (args.keepIntermediateFiles) {
                            let transientFile = path.join(args.outputDirectory, path.basename(filePath));
                            root.createFile(transientFile, outFile.contents, "intermediateFile");
                        }

                        root.minifyJs(outFile, {
                            suffix: args.suffix,
                            outputDirectory: args.outputDirectory,
                            sourceMapDirectory: args.sourceMapDirectory,
                            generateSourceMaps: args.generateSourceMaps
                        }, onSuccess);
                        break;
                }
            }
        });

        let usedDict = {}, key, err;
        let diagnostic = ts.getPreEmitDiagnostics(compiler).concat(emitResult.diagnostics);
        for (var i = 0; i < diagnostic.length; i++) {
            err = diagnostic[i];
            if (err.file) {
                var position = err.file.getLineAndCharacterOfPosition(err.start);
                let description = ts.flattenDiagnosticMessageText(err.messageText, '\n');
                key = (err.file.fileName + position.line + position.character);

                if (usedDict.hasOwnProperty(key) === false) {
                    usedDict[key] = true;
                    console.error(JSON.stringify({
                        code: err.code,
                        file: err.file.fileName,
                        line: (position.line + 1),
                        column: (position.character + 1),
                        message: description.replace(/\s/, " ")
                    }));
                }
            }
        }

        if (emitResult.emitSkipped) {
            onSuccess();
            throw "Compilation errors detected.";
        }
    },

    minifyJs: function (sourceFile, args, onSuccess) {
        let outFile = path.join(args.outputDirectory, (path.basename(sourceFile.path, ".js") + args.suffix + ".js"));
        let mapFile = path.join(args.sourceMapDirectory, (path.basename(outFile) + ".map"));

        let options = {
            ie8: true
        };
        if (args.generateSourceMaps) {
            options.sourceMap = {
                filename: mapFile,
                root: args.outputDirectory,
                url: path.relative(args.outputDirectory, mapFile)
            }
        }

        var minifyResult = uglify.minify(sourceFile.contents, options);

        if (args.generateSourceMaps) {
            sourceFile.sourceMap = root.mergeSourceMaps(minifyResult.map, sourceFile.sourceMap, path.relative(args.sourceMapDirectory, outFile));
            root.createFile(mapFile, sourceFile.sourceMap, "sourceMapFile");
        }

        root.createFile(outFile, minifyResult.code, "minifiedFile", onSuccess);
    },

    compileSass: function (sourceFile, args, onSuccess) {
        let outFile = new root.FileInfo(args.outputFile);
        let mapFile = path.join(args.sourceMapDirectory, (path.basename(outFile.path) + ".map"));

        let options = {
            file: sourceFile,
            sourceMap: mapFile,
            sourceComments: false,
            outFile: outFile.path,
            outputStyle: "expanded",
            omitSourceMapUrl: (!args.generateSourceMaps)
        };

        sass.render(options, function (err, result) {
            if (err) {
                console.error(JSON.stringify({
                    file: err.file,
                    line: err.line,
                    code: err.status,
                    column: err.column,
                    message: err.message
                }));
                process.exit(err.status);
            }
            else {
                outFile.contents = result.css.toString();
                outFile.sourceMap = result.map.toString();

                if (args.keepIntermediateFiles) {
                    root.createFile(outFile.path, result.css, "intermediateFile");

                    if (args.generateSourceMaps) {
                        root.createFile(mapFile, result.map, "sourceMapFile2");
                    }
                }

                root.minifyCss(outFile, {
                    suffix: args.suffix,
                    outputDirectory: args.outputDirectory,
                    sourceMapDirectory: args.sourceMapDirectory,
                    generateSourceMaps: args.generateSourceMaps
                }, onSuccess);
            }
        });
    },

    minifyCss: function (sourceFile, args, onSuccess) {
        let outFile = path.join(args.outputDirectory, (path.basename(sourceFile.path, ".css") + args.suffix + ".css"));
        let mapFile = path.join(args.sourceMapDirectory, (path.basename(outFile) + ".map"));
        let result = csso.minify(sourceFile.contents, {
            filename: sourceFile.path,
            sourceMap: args.generateSourceMaps
        });

        sourceFile.path = outFile;
        sourceFile.contents = result.css;

        if (args.generateSourceMaps) {
            sourceFile.contents = (result.css + os.EOL + "/* # sourceMappingURL=" + path.relative(args.outputDirectory, mapFile) + " */");
            let map = root.mergeSourceMaps(result.map.toString(), sourceFile.sourceMap, path.relative(args.sourceMapDirectory, outFile));
            root.createFile(mapFile, map, "sourceMapFile");
        }

        root.createFile(outFile, sourceFile.contents, "minifiedFile", onSuccess);
    },

    /* ===== HELPERS ===== */

    changeSourceMapUrl: function (contents, url) {
        let pattern = /sourceMappingURL\s*=\s*([a-z 0-9\.\\\/_:-]+)/i;
        if (pattern.test(contents)) {
            return contents.replace(pattern, ("sourceMapUrl=" + url));
        }

        return contents;
    },

    mergeSourceMaps: function (mapB, mapA, targetFile) {
        if (mapA) {
            var mapC = mssMerger.transfer({
                fromSourceMap: mapB,
                toSourceMap: mapA
            });

            let map = JSON.parse(mapC.toString());
            map.file = targetFile;
            return JSON.stringify(map, null, 2);
        }

        return mapB;
    },

    createFile: function (filePath, contents, label, done) {
        fs.writeFile(filePath, contents, function (err) {
            if (err) {
                console.debug(err);
            }

            if (label) {
                process.env["foo"] = filePath;
                let obj = {};
                obj[label] = filePath;
                console.log(JSON.stringify(obj));
            }

            if (done) { done(); }
        });
    },

    // STRUCTS
    // ==========================================================================================

    FileInfo: function (filePath, contents, map) {
        this.contents = contents;
        this.sourceMap = map;
        this.path = filePath;
    }
};

module.exports = root;