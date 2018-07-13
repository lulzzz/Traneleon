/// <reference path="script2.ts" />
var Test;
(function (Test) {
    var Page = /** @class */ (function () {
        function Page() {
            this.init("unitest");
        }
        Page.prototype.init = function (text) {
            var dud;
            console.log("Invoked init with arg: '" + text + "'");
            var compoent = new Test.Component("button");
            compoent.action();
        };
        return Page;
    }());
    Test.Page = Page;
})(Test || (Test = {}));
var page = new Test.Page();
// this comment shouldn't be here.
//# sourceMapUrl=..\..\..\..\..\..\AppData\Local\Temp\script1.js.map