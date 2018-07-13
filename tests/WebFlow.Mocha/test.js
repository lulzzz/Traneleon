const os = require("os");
const fs = require("fs");
const path = require("path");
const expect = require("expect.js");
const webFlow = require("../../src/WebFlow/Scripts/webflow");

const resultsDirectory = path.join(os.tmpdir(), "WebFlow");
const testDataDirectory = path.resolve("../WebFlow.MSTest/TestData");

describe("WebFlow", function () {
    before(function () {
        if (!fs.existsSync(resultsDirectory)) {
            fs.mkdirSync(resultsDirectory);
        }
    });

    describe("compileTs", function () {
        it("can_compile_a_typscript_file", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.bundling = false;
            options.suffix = ".min";
            options.bundling = false;
            options.outputFile = false;
            options.generateSourceMaps = true;
            options.keepIntermediateFiles = true;
            options.sourceMapDirectory = os.tmpdir();
            options.sourceFiles = [path.join(testDataDirectory, "script1.ts")];
            options.outputFile = path.join(resultsDirectory, "script1.js");
            options.outputDirectory = path.dirname(options.outputFile);

            webFlow.compileTs(options, done);
        });

        it("can_compile_a_typescript_bundle", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.bundling = true;
            options.suffix = ".min";
            options.bundling = true;
            options.generateSourceMaps = true;
            options.keepIntermediateFiles = true;
            options.sourceMapDirectory = os.tmpdir();
            options.outputFile = path.join(resultsDirectory, "tsc-build2.js");
            options.outputDirectory = path.dirname(options.outputFile);
            options.sourceFiles = [
                path.join(testDataDirectory, "script1.ts"),
                path.join(testDataDirectory, "script2.ts")
            ];

            webFlow.compileTs(options, done);
        })

        it("can_report_errors", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.bundling = false;
            options.suffix = ".min";
            options.generateSourceMaps = true;
            options.keepIntermediateFiles = true;
            options.sourceMapDirectory = os.tmpdir();
            options.sourceFiles = [path.join(testDataDirectory, "bad_script1.ts")];
            options.outputFile = path.join(resultsDirectory, "bad_script1.js");
            options.outputDirectory = path.dirname(options.outputFile);

            expect(function () { webFlow.compileTs(options, done); }).to.throwException();
        });
    });

    describe("compileSass", function () {
        it("can_compile_a_sass_file", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.suffix = ".min";
            options.outputFile = path.join(resultsDirectory, "style1.css");
            options.sourceFiles = [path.join(testDataDirectory, "style1.scss")];
            options.outputDirectory = path.dirname(options.outputFile);
            options.sourceMapDirectory = os.tmpdir();
            options.keepIntermediateFiles = true;
            options.generateSourceMaps = true;
            
            webFlow.compileSass(options.sourceFiles[0], options, done);
        });

        it.skip("can_report_errors", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.suffix = ".min";
            options.sourceMapDirectory = os.tmpdir();
            options.sourceFiles = [path.join(testDataDirectory, "bad_style1.scss")];
            options.outputFile = path.join(resultsDirectory, "bad_style1.css");
            options.outputDirectory = path.dirname(options.outputFile);
            options.keepIntermediateFiles = true;
            options.generateSourceMaps = true;
            
            expect(function () { webFlow.compileSass(options.sourceFiles[0], options, done); }).to.throwException();
        });
    });
});