const fs = require("fs");
const Path = require("path");
const TsCompiler = require("typescript");
const Uglify = require("uglify-js");
const MapMerger = require("multi-stage-sourcemap");

var root = {
    // #region OPTIONS
    // ==========================================================================================

    TranspilierOptions: function () {
        return {
            suffix: process.argv[7],
            outputDirectory: process.argv[6],
            sourceMapDirectory: process.argv[8],
            sourceFile: (new root.FileInfo(process.argv[5])),
            bundle: (process.argv[2].toLowerCase() == "true"),
            generateSourceMaps: (process.argv[4].toLowerCase() == "true"),
            keepIntermediateFiles: (process.argv[3].toLowerCase() == "true")
        };
    },

    // #endregion

    // METHODS
    // ==========================================================================================

    compileTs: function (sourceFile, args) {
        let options = {
            allowJs: true,
            sourceMap: true
        };
        if (args.bundle) {
            options["outFile"] = Path.join(args.outputDirectory, (Path.basename(sourceFile.path, ".ts") + ".js"))
        }

        var compiler = TsCompiler.createProgram([sourceFile.path], options);;
        compiler.emit(null, function (filePath, contents) {
            switch (Path.extname(filePath).toLowerCase()) {
                case ".map":
                    let targetFile = Path.join(args.outputDirectory, Path.basename(filePath, ".map"));
                    sourceFile.sourceMap = root.normalizeSourceMap(contents, targetFile, args.sourceMapDirectory);

                    if (args.keepIntermediateFiles && args.generateSourceMaps) {
                        var mapFile = Path.join(args.sourceMapDirectory, (Path.basename(targetFile) + ".map"));
                        root.createFile(mapFile, sourceFile.sourceMap, "sourceMapFile2");
                    }
                    break;

                case ".js":
                    sourceFile.contents = contents;
                    sourceFile.path = Path.join(args.outputDirectory, Path.basename(filePath));
                    if (args.generateSourceMaps) { root.fixSourceMapUrl(sourceFile, args.sourceMapDirectory); }

                    root.minifyJs(sourceFile, {
                        suffix: args.suffix,
                        outputDirectory: args.outputDirectory,
                        sourceMapDirectory: args.sourceMapDirectory,
                        generateSourceMaps: args.generateSourceMaps
                    });

                    if (args.keepIntermediateFiles) {
                        root.createFile(sourceFile.path, contents, "intermediateFile");
                    }
                    break;
            }
        });
    },

    minifyJs: function (sourceFile, args) {
        var result = Uglify.minify(sourceFile.contents, {
            ie8: true,
            sourceMap: true
        });

        sourceFile.contents = result.code;
        sourceFile.path = Path.join(args.outputDirectory, (Path.basename(sourceFile.path, ".js") + args.suffix + ".js"));

        if (args.generateSourceMaps) {
            root.fixSourceMapUrl(sourceFile, args.sourceMapDirectory);
            sourceFile.sourceMap = root.mergeSourceMap(result.map.toString(), sourceFile.sourceMap);
            root.createFile((sourceFile.path + ".map"), sourceFile.sourceMap, "sourceMapFile");
        }

        this.createFile(sourceFile.path, sourceFile.contents, "minifiedFile");
    },

    /* ===== Utilities ===== */

    fixSourceMapUrl: function (sourceFile, sourceMapDirectory) {
        // Ensure the source map url in the .js file is good.
    },

    normalizeSourceMap: function (contents, targetFile, sourceMapDirectory) {
        var obj = JSON.parse(contents);
        obj.file = Path.relative(sourceMapDirectory, Path.resolve(targetFile));
        for (var i = 0; i < obj.sources.length; i++) {
            obj.sources[i] = Path.relative(sourceMapDirectory, Path.resolve(obj.sources[i]));
        }
        return JSON.stringify(obj, null, 2);
    },

    mergeSourceMap: function (from, to) {
        if (to) {
            let sourceMap = MapMerger.transfer({
                fromSourceMap: from,
                toSourceMap: to
            });

            return JSON.stringify(JSON.parse(sourceMap), null, 2);
        }

        return from;
    },

    createFile: function (filePath, contents, label) {
        fs.writeFile(filePath, contents, function (err) {
            if (err) {
                process.exit(err.errno);
            }

            if (label) {
                var obj = {};
                obj[label] = filePath;
                console.log(JSON.stringify(obj));
            }
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