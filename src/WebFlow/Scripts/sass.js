var webFlow = require("./webflow");

var options = new webFlow.TranspilierOptions();
webFlow.compileSass(options.sourceFiles[0], options);