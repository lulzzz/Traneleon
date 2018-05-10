var webFlow = require("./webflow");

var options = new webFlow.TranspilierOptions();
webFlow.compileTs(options.sourceFile, options);