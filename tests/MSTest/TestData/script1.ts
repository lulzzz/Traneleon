/// <reference path="script2.ts" />

namespace Test {
    export class Page {
        constructor() {
            this.init("unitest");
        }

        init(text: string): void {
            var dud;
            console.log(`Invoked init with arg: '${text}'`);
            var compoent = new Test.Component("button");
            compoent.action();
        }
    }
}

var page = new Test.Page();
// this comment shouldn't be here.