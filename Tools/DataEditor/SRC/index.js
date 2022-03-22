const ElementUI = require("element-ui")

Vue.use(ElementUI.Col)
Vue.use(ElementUI.Menu)
Vue.use(ElementUI.Table)
Vue.use(ElementUI.TableColumn)
Vue.use(ElementUI.MenuItem)
Vue.use(ElementUI.Card)

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