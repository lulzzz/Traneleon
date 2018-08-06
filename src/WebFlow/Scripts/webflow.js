const os = require("os");
const fs = require("fs");
const path = require("path");
const csso = require("csso");
const ts = require("typescript");
const sass = require("node-sass");
const uglify = require("uglify-js");
//const convert = require("convert-source-map");
//const combine = require("combine-source-map");
const mssMerger = require("multi-stage-sourcemap").transfer;

var root = {
    // #region OPTIONS
    // ==========================================================================================

    TranspilierOptions: function () {
        var me = this;

        let pattern = /true/i;
        me.sourceFile = process.argv[4];
        me.bundling = pattern.test(process.argv[8]);
        me.generateSourceMaps = (pattern.test(process.argv[3]));
        me.keepIntermediateFiles = (pattern.test(process.argv[2]));

        me.suffix = (process.argv[6] == "false" ? false : process.argv[6]);
        me.outputFile = (process.argv[5] == "false" ? false : process.argv[5]);
        me.outputDirectory = (process.argv[9] == "false" ? false : process.argv[9]);
        me.sourceMapDirectory = (process.argv[7] == "false" ? false : process.argv[7]);
    },

    // #endregion

    // METHODS
    // ==========================================================================================

    compileTs: function (args, callback) {
        let fileNameWithoutExtension, outDir, outFile;

        if (args.bundling) {
            outDir = path.dirname(args.outputFile);
            fileNameWithoutExtension = path.basename(args.outputFile, path.extname(args.outputFile));
            outFile = new root.FileInfo(path.join(outDir, (fileNameWithoutExtension + ".js")));
        }
        else {
            outDir = (args.outputDirectory ? args.outputDirectory : path.dirname(args.sourceFile));
            fileNameWithoutExtension = path.basename(args.sourceFile, path.extname(args.sourceFile));
            outFile = new root.FileInfo(path.join(outDir, (fileNameWithoutExtension + ".js")));
        }

        let mapDir = (args.sourceMapDirectory ? args.sourceMapDirectory : outDir);
        let mapFile = path.join(mapDir, (path.basename(outFile.path) + ".map"));

        let options = {};
        options.noEmitOnError = true;

        if (args.generateSourceMaps) {
            options["mapRoot"] = mapDir;
            options["sourceMap"] = args.generateSourceMaps;
        }

        if (args.bundling) {
            options["allowJs"] = true;
            options["outFile"] = outFile.path;
        }

        let compiler = ts.createProgram(args.sourceFile.split(';'), options);
        let target = (args.bundling ? null : compiler.getSourceFile(outFile.path));
        let emitResult = compiler.emit(target, function (filePath, contents) {
            if (path.basename(filePath).startsWith(fileNameWithoutExtension)) {
                switch (path.extname(filePath).toLowerCase()) {
                    case ".map":
                        let map = JSON.parse(contents);
                        map.file = path.relative(mapDir, outFile.path);
                        outFile.sourceMap = JSON.stringify(map, null, 2);
                        if (args.generateSourceMaps && args.keepIntermediateFiles) {
                            root.createFile(path.join(mapDir, path.basename(filePath)), outFile.sourceMap, "sourceMapFile2");
                        }
                        break;

                    case ".js":
                        outFile.path = filePath;
                        outFile.contents = root.changeSourceMapUrl(contents, path.relative(outDir, mapFile));

                        if (args.keepIntermediateFiles) {
                            let transientFile = path.join(outDir, path.basename(filePath));
                            root.createFile(transientFile, outFile.contents, "intermediateFile");
                        }

                        root.minifyJs(outFile, {
                            suffix: args.suffix,
                            outputDirectory: outDir,
                            sourceMapDirectory: mapDir,
                            generateSourceMaps: args.generateSourceMaps
                        }, callback);
                        break;
                }
            }
        });

        let duplicateErrors = {}, key, err;
        let diagnostic = ts.getPreEmitDiagnostics(compiler).concat(emitResult.diagnostics);
        for (var i = 0; i < diagnostic.length; i++) {
            err = diagnostic[i];
            if (err.file) {
                let position = err.file.getLineAndCharacterOfPosition(err.start);
                let description = ts.flattenDiagnosticMessageText(err.messageText, '\n');
                key = (err.file.fileName + position.line + position.character);

                if (duplicateErrors.hasOwnProperty(key) === false && path.extname(err.file.fileName) === ".ts") {
                    duplicateErrors[key] = true;

                    console.error(JSON.stringify({
                        code: err.code,
                        file: err.file.fileName,
                        line: (position.line + 1),
                        column: (position.character + 1),
                        message: description.replace(/\s/, " "),
                        category: root.getErrorCategory(err.category)
                    }));
                }
            }
        }

        if (emitResult.emitSkipped) {
            callback(emitResult);
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

    compileSass: function (args, callback) {
        let sourceFile = args.sourceFile;
        let fileNameWithoutExtension = path.basename(sourceFile, path.extname(sourceFile));
        let outDir = (args.outputDirectory ? args.outputDirectory : path.dirname(sourceFile));
        let outFile = new root.FileInfo(path.join(outDir, (fileNameWithoutExtension + ".css")));

        let mapDir = (args.sourceMapDirectory ? args.sourceMapDirectory : outDir);
        let mapFile = path.join(mapDir, (fileNameWithoutExtension + ".css.map"));

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
                    code: err.status,
                    column: err.column,
                    message: err.message,
                    line: (err.line - 1),
                }));
                callback(err);
                throw err.message;
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
                    outputDirectory: outDir,
                    sourceMapDirectory: mapDir,
                    generateSourceMaps: args.generateSourceMaps
                }, callback);
            }
        });
    },

    minifyCss: function (sourceFile, args, callback) {
        let fileNameWitoutExtension = path.basename(sourceFile.path, path.extname(sourceFile.path));
        let outDir = (args.outputDirectory ? args.outputDirectory : path.dirname(sourceFile.path));
        let outFile = path.join(outDir, (fileNameWitoutExtension + args.suffix + ".css"));

        let mapDir = (args.sourceMapDirectory ? args.sourceMapDirectory : outDir);
        let mapFile = path.join(mapDir, (path.basename(outFile) + ".map"));

        let result = csso.minify(sourceFile.contents, {
            filename: sourceFile.path,
            sourceMap: args.generateSourceMaps
        });

        sourceFile.path = outFile;
        sourceFile.contents = result.css;

        if (args.generateSourceMaps) {
            sourceFile.contents = (result.css + os.EOL + "/*# sourceMappingURL=" + path.relative(outDir, mapFile) + " */");
            let map = root.mergeSourceMaps(result.map.toString(), sourceFile.sourceMap, path.relative(mapDir, outFile));
            root.createFile(mapFile, map, "sourceMapFile");
        }

        root.createFile(outFile, sourceFile.contents, "minifiedFile", callback);
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
            var mapC = mssMerger({
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
        var dir = path.dirname(filePath);
        fs.access(dir, (fs.constants.F_OK | fs.constants.W_OK), function (err) {
            if (err) {
                fs.mkdirSync(dir);
            }

            fs.writeFile(filePath, contents, function (err) {
                if (err) {
                    console.debug(err);
                }

                if (label) {
                    let obj = {};
                    obj[label] = filePath;
                    console.log(JSON.stringify(obj));
                }

                if (done) { done(); }
            });
        });
    },

    getErrorCategory: function (value) {
        let category;
        switch (value) {
            case 1:
                category = 0; /* error */
                break;

            case 0:
                category = 1; /* warn */
                break;

            default:
                category = 2; /* info */
                break;
        }

        return category;
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