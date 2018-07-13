var Test;
(function (Test) {
    var Component = /** @class */ (function () {
        function Component(name) {
            this.name = name;
        }
        Component.prototype.action = function () {
            console.log("invoked action: " + this.name);
        };
        return Component;
    }());
    Test.Component = Component;
})(Test || (Test = {}));
//# sourceMapUrl=..\..\..\..\..\..\AppData\Local\Temp\script2.js.map