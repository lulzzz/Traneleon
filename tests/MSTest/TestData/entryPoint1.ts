/// <reference path="script2.ts" />

namespace Test {
    export class App {
        constructor(name: string) {
            this.element = new Test.Component(name);
        }

        element: Component;

        run(): void {
            this.element.action();
        }
    }
}