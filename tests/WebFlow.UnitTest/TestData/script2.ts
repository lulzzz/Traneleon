namespace Test {
    export class Component {
        constructor(name) {
            this.name = name;
        }

        name: string;

        action(): void {
            console.log(`invoked action: ${this.name}`);
        }
    }
}