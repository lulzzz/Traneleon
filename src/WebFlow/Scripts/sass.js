var webFlow = require("./webflow");

var options = new webFlow.TranspilierOptions();
webFlow.compileSass(options.sourceFile, options);