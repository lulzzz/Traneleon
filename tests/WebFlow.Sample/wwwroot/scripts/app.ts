/// <reference path="components/button.ts" />

namespace Site {
    export class App {
        constructor() {
            this.button1 = new Button("js_btn1", "click me");
            this.button1.action();

            console.log("app initialized.");
        }

        static getData(): void {
            console.log("invoked");
        }

        button1: Button;
    }
}

var vm = new Site.App();
// do yuck foo bar foo bar foo
// foo roo doo foo scoop poop poop whoop scoop