const ElementUI = require("element-ui")

Vue.use(ElementUI.Col)
Vue.use(vueNcform, { extComponents: ncformStdComps, /*lang: 'zh-cn'*/ });

// Bootstrap the app
new Vue({
    el: '#App',
    data: {
        formSchema: {
            type: 'object',
            properties: {
                name: {
                    type: 'string'
                }
            }
        }
    },
    methods: {
        submit() {
            this.$ncformValidate('form1').then(data => {
                if (data.result) {
                    // do what you like to do
                }
            });
        }
    }
});