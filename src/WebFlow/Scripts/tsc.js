var webFlow = require("./webflow");

var options = new webFlow.TranspilierOptions();
let results = webFlow.compileTs(options.sourceFile, options);