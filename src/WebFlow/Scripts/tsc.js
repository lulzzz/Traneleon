var Lib = require("./lib");

let args = new Lib.TranspilierOptions();
let results = Lib.compileTs(args.sourceFile, args);