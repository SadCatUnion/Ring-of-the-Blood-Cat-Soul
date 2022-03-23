const ElementUI = require("element-ui")
const path = require("path")

const amdLoader = require("monaco-editor/min/vs/loader.js");
const amdRequire = amdLoader.require;
const amdDefine = amdLoader.require.define;

function uriFromPath(_path) {
    var pathName = path.resolve(_path).replace(/\\/g, '/');
    if (pathName.length > 0 && pathName.charAt(0) !== '/') {
        pathName = '/' + pathName;
    }
    return encodeURI('file://' + pathName);
}

amdRequire.config({
    baseUrl: uriFromPath(path.join(__dirname, './node_modules/monaco-editor/min'))
});

let editor


Vue.use(ElementUI.Col)
Vue.use(ElementUI.Menu)
Vue.use(ElementUI.Table)
Vue.use(ElementUI.TableColumn)
Vue.use(ElementUI.MenuItem)
Vue.use(ElementUI.Card)
Vue.use(ElementUI.Button)
Vue.use(ElementUI.ButtonGroup)
Vue.use(ElementUI.Input)

Vue.use(vueNcform, { extComponents: ncformStdComps, /*lang: 'zh-cn'*/ });

// Bootstrap the app
new Vue({
    el: '#App',
    data: {
        formSchema: {
            "type": "object",
            "properties": {
                "ID": {
                    "type": "string"
                },
                "Key": {
                    "type": "string"
                },
                "Desc": {
                    "type": "integer"
                },
                "Value1": {
                    "type": "boolean"
                }
            }
        },
        tableData: [{
            ID: 1,
            Key: 'A',
            Desc: 'desc1'
        }, {
            ID: 2,
            Key: 'B',
            Desc: 'desc2'
        }, {
            ID: 3,
            Key: 'C',
            Desc: 'desc3'
        }, {
            ID: 4,
            Key: 'D',
            Desc: 'desc4'
        }, {
            ID: 5,
            Key: 'E',
            Desc: 'desc5'
        },]
    },
    methods: {
        submit() {
            this.$ncformValidate('form1').then(data => {
                if (data.result) {
                    // do what you like to do
                }
            });
        },
        handleOpen(key, keyPath) {
            console.log(key, keyPath);
        },
        handleClose(key, keyPath) {
            console.log(key, keyPath);
        },

    }
});

amdRequire(['vs/editor/editor.main'], function () {
    editor = monaco.editor.create(document.getElementById('CodeContainer'), {
        value: '{}',
        language: 'json',
        theme: "vs-light",
        automaticLayout: true,//自动布局
        fontSize: 18
    });
});
