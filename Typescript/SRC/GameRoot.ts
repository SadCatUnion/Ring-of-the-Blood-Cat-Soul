import {JsBehaviour, UnityEngine} from "csharp"
import {HelloWorld} from "TESTMod/MyMod"

class GameRoot {
    bindTo: JsBehaviour
    constructor(bindTo: JsBehaviour) {
        this.bindTo = bindTo;
        this.bindTo.JsStart = () => this.onStart();
        this.bindTo.JsUpdate = () => this.onUpdate();
        this.bindTo.JsOnDestroy = () => this.onDestroy();
    }

    onStart() {

    }
    
    onUpdate() {
        
    }
    
    onDestroy() {

    }
}

function init(bindTo: JsBehaviour): void {
    new GameRoot(bindTo)
}

export {init}