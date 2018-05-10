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
        let sample = path.join(testDataDirectory, "script1.ts");

        it("can_compile_a_typscript_file", function (done) {
            var options = new webFlow.TranspilierOptions();
            options.suffix = ".min";
            options.outputDirectory = resultsDirectory;
            options.sourceMapDirectory = os.tmpdir();
            options.keepIntermediateFiles = true;
            options.generateSourceMaps = true;
            options.concat = false;

            webFlow.compileTs(new webFlow.FileInfo(sample), options, done);
        });
    });
});