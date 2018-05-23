/// <reference path="components/button.ts" />

namespace Site {
    export class App {
        constructor() {
            this.button1 = new Button("js_btn1", "click me");
            this.button1.action();
        }

        static getData(): void {
            console.log("data got got");
        }

        button1: Button;
    }
}