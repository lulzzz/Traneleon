const fs = require("fs");
const Path = require("path");
const Uglify = require("uglify-js");
const ts = require("typescript");
const MapMerger = require("multi-stage-sourcemap");

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
        let outFile = Path.join(args.outputDirectory, (Path.basename(sourceFile.path, ".ts") + ".js"));
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
            switch (Path.extname(filePath).toLowerCase()) {
                case ".map":
                    let map = JSON.parse(contents);
                    map.file = Path.relative(args.sourceMapDirectory, outFile);
                    sourceFile.sourceMap = JSON.stringify(map, null, 2);
                    if (args.generateSourceMaps && args.keepIntermediateFiles) {
                        root.createFile(Path.join(args.sourceMapDirectory, Path.basename(filePath)), sourceFile.sourceMap, "sourceMapFile2");
                    }
                    break;

                case ".js":
                    let mapPath = Path.join(args.sourceMapDirectory, (Path.basename(filePath) + ".map"));
                    sourceFile.contents = root.changeSourceMapUrl(contents, Path.relative(args.outputDirectory, mapPath));
                    sourceFile.path = filePath;

                    if (args.keepIntermediateFiles) {
                        let transientFile = Path.join(args.outputDirectory, Path.basename(filePath));
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
                    message: description,
                    line: (position.line + 1),
                    column: (position.character + 1)
                }));
            }
        }
    },

    minifyJs: function (sourceFile, args, onSuccess) {
        let outFile = Path.join(args.outputDirectory, (Path.basename(sourceFile.path, ".js") + args.suffix + ".js"));
        let mapFile = Path.join(args.sourceMapDirectory, (Path.basename(outFile) + ".map"));

        let options = {
            ie8: true
        };
        if (args.generateSourceMaps) {
            options.sourceMap = {
                filename: mapFile,
                root: args.outputDirectory,
                url: Path.relative(args.outputDirectory, mapFile)
            }
        }

        var minifyResult = Uglify.minify(sourceFile.contents, options);

        if (args.generateSourceMaps) {
            sourceFile.sourceMap = root.mergeSourceMaps(minifyResult.map, sourceFile.sourceMap, Path.relative(args.sourceMapDirectory, outFile));
            root.createFile(mapFile, sourceFile.sourceMap, "sourceMapFile");
        }

        root.createFile(outFile, minifyResult.code, "minifiedFile", onSuccess);
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
            var mapC = MapMerger.transfer({
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