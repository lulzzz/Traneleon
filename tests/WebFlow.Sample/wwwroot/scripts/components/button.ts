namespace Site {
    export class Button {
        constructor(id: string, name: string) {
            var self = this;
            this.name = name;

            document.getElementById(id).addEventListener("click", function (e) {
                if (this === e.target) {
                    self.action();
                }
            });
        }

        name: string;

        action(): void {
            console.log(`button['${this.name}'] was invoked.`);
        }
    }
}