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
        this.suffix = process.argv[7];
        this.outputDirectory = process.argv[6];
        this.sourceMapDirectory = process.argv[8];
        this.sourceFile = (new root.FileInfo(process.argv[5]));

        let pattern = /true/i;
        this.concat = (pattern.test(process.argv[2]));
        this.generateSourceMaps = (pattern.test(process.argv[4]));
        this.keepIntermediateFiles = (pattern.test(process.argv[3]));
    },

    // #endregion

    // METHODS
    // ==========================================================================================

    compileTs: function (sourceFile, args, onSuccess) {
        let outFile = path.join(args.outputDirectory, (path.basename(sourceFile.path, ".ts") + ".js"));
        let options = {
            sourceMap: true,
            noEmitOnError: true,
            mapRoot: args.sourceMapDirectory
        };
        if (args.concat) {
            options["allowJs"] = true;
            options["outFile"] = outFile;
        }

        let compiler = ts.createProgram([sourceFile.path], options);
        let target = (options.outFile ? null : compiler.getSourceFile(sourceFile.path));
        let emitResult = compiler.emit(target, function (filePath, contents, bom, onError, inputFiles) {
            switch (path.extname(filePath).toLowerCase()) {
                case ".map":
                    let map = JSON.parse(contents);
                    map.file = path.relative(args.sourceMapDirectory, outFile);
                    sourceFile.sourceMap = JSON.stringify(map, null, 2);
                    if (args.generateSourceMaps && args.keepIntermediateFiles) {
                        root.createFile(path.join(args.sourceMapDirectory, path.basename(filePath)), sourceFile.sourceMap, "sourceMapFile2");
                    }
                    break;

                case ".js":
                    let mapPath = path.join(args.sourceMapDirectory, (path.basename(filePath) + ".map"));
                    sourceFile.contents = root.changeSourceMapUrl(contents, path.relative(args.outputDirectory, mapPath));
                    sourceFile.path = filePath;

                    if (args.keepIntermediateFiles) {
                        let transientFile = path.join(args.outputDirectory, path.basename(filePath));
                        root.createFile(transientFile, sourceFile.contents, "intermediateFile");
                    }

                    root.minifyJs(sourceFile, {
                        suffix: args.suffix,
                        outputDirectory: args.outputDirectory,
                        sourceMapDirectory: args.sourceMapDirectory,
                        generateSourceMaps: args.generateSourceMaps
                    }, onSuccess);
                    break;
            }
        });

        var diagnostic = ts.getPreEmitDiagnostics(compiler).concat(emitResult.diagnostics);
        for (var i = 0; i < diagnostic.length; i++) {
            err = diagnostic[i];
            if (err.file) {
                var position = err.file.getLineAndCharacterOfPosition(err.start);
                let description = ts.flattenDiagnosticMessageText(err.messageText, '\n');

                console.error(JSON.stringify({
                    code: err.code,
                    file: err.file.fileName,
                    line: (position.line + 1),
                    column: (position.character + 1),
                    message: description.replace(/\s/, " ")
                }));
            }
        }

        if (emitResult.emitSkipped) {
            process.exit(emitResult.emitSkipped ? 1 : 0);
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
        let outFile = path.join(args.outputDirectory, (path.basename(sourceFile.path, path.extname(sourceFile.path)) + ".css"));
        let mapFile = path.join(args.sourceMapDirectory, (path.basename(outFile) + ".map"));

        let options = {
            outFile: outFile,
            sourceMap: mapFile,
            sourceComments: false,
            file: sourceFile.path,
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
                sourceFile.path = outFile;
                sourceFile.contents = result.css.toString();
                sourceFile.sourceMap = result.map.toString();

                if (args.keepIntermediateFiles) {
                    root.createFile(outFile, result.css, "intermediateFile");

                    if (args.generateSourceMaps) {
                        root.createFile(mapFile, result.map, "sourceMapFile2");
                    }
                }

                root.minifyCss(sourceFile, {
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