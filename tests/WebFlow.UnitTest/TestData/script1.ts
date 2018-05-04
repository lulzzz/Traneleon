namespace Test {
    export class Page {
        constructor() {
            this.action("unitest");
        }

        action(text: string): void {
            var dud;
            console.log(`Invoked action with arg: '${text}'`);
        }
    }
}

var page = new Test.Page();
// this comment shouldn't be here.