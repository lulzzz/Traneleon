const os = require("os");
const fs = require("fs");
const path = require("path");
const expect = require("expect.js");
const webFlow = require("../../src/WebFlow/Scripts/webflow");

const resultsDirectory = path.join(os.tmpdir(), "webflow-mocha");
const testDataDirectory = path.resolve("../WebFlow.MSTest/TestData");

describe("WebFlow", function () {
    describe.skip("compileTs", function () {
        it("can_compile_a_typscript_file", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.bundling = false;
            options.generateSourceMaps = true;
            options.keepIntermediateFiles = true;

            options.suffix = ".min";
            options.sourceMapDirectory = os.tmpdir();
            options.outputDirectory = resultsDirectory;
            options.sourceFile = path.join(testDataDirectory, "script1.ts");

            webFlow.compileTs(options, done);
        });

        it("can_compile_a_typescript_bundle", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.bundling = true;
            options.generateSourceMaps = false;
            options.keepIntermediateFiles = true;

            options.suffix = ".min";
            options.sourceMapDirectory = os.tmpdir();
            options.outputDirectory = resultsDirectory;
            options.outputFile = path.join(resultsDirectory, "build.js");
            options.sourceFile = [
                path.join(testDataDirectory, "script1.ts"),
                path.join(testDataDirectory, "script2.ts")
            ].join(';');

            webFlow.compileTs(options, done);
        });

        it.skip("can_report_errors", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.bundling = false;
            options.suffix = ".min";
            options.generateSourceMaps = true;
            options.keepIntermediateFiles = true;
            options.sourceMapDirectory = os.tmpdir();
            options.sourceFile = path.join(testDataDirectory, "bad_script1.ts");
            options.outputFile = path.join(resultsDirectory, "bad_script1.js");
            options.outputDirectory = path.dirname(options.outputFile);

            expect(function () { webFlow.compileTs(options, done); }).to.throwException();
        });
    });

    describe.skip("compileSass", function () {
        it("can_compile_a_sass_file", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.generateSourceMaps = true;
            options.keepIntermediateFiles = true;

            options.suffix = ".min";
            options.outputDirectory = resultsDirectory;
            options.sourceMapDirectory = os.tmpdir();
            options.sourceFile = path.join(testDataDirectory, "style1.scss");

            webFlow.compileSass(options, done);
        });

        it.skip("can_report_errors", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.generateSourceMaps = true;
            options.keepIntermediateFiles = true;

            options.suffix = ".min";
            options.outputDirectory = resultsDirectory;
            options.sourceMapDirectory = resultsDirectory;
            options.sourceFile = path.join(testDataDirectory, "bad_style1.scss");

            expect(function () { webFlow.compileSass(options, done); }).to.throwException();
        });
    });

    describe("manual_tests", function () {
        it("source map", function (done) {
            var arg = new webFlow.TranspilierOptions();
            arg.sourceFile = "C:/Users/Ackeem/Projects/Ackara/WebFlow/tests/WebFlow.Sample/wwwroot/scripts/app.ts;C:/Users/Ackeem/Projects/Ackara/WebFlow/tests/WebFlow.Sample/wwwroot/scripts/components/button.ts";
            arg.outputFile = "C:/Users/Ackeem/AppData/Local/Temp/build.ts";
            arg.outputDirectory = "C:/Users/Ackeem/AppData/Local/Temp/";
            arg.keepIntermediateFiles = true;
            arg.generateSourceMaps = true;
            arg.bundling = true;
            arg.suffix = ".min";

            webFlow.compileTs(arg, done);
        });
    });
});