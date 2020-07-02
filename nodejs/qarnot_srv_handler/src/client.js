"use strict";

const axios = require('axios').default;


class QarnotSrvHandler {
    constructor(axios, url) {
        this.axios = axios; // const
        this.url = url; // const
        this.config = null; // var
        axios.interceptors.request.use((request) => { return this.intercept_request_success(request)}, (error) => { return this.intercept_request_error(error) })
        axios.interceptors.response.use((response) => { return this.intercept_response_success(response) }, (error) => { return this.intercept_response_error(error) })
    }

    intercept_request_error(error)
    {
        return error
    }

    intercept_request_success(config)
    {
        this.config = config
        const url_path = this.get_uri_path(config.url)
        // console.log(config)
        config = this.set_new_uri(config, url_path)

        console.log(config)
        return config
    }

    get_new_url(config)
    {
        return config.url
    }

    set_new_uri(config, url_path)
    {
        const new_url = this.get_new_url(config)
        config.url = new_url + "/" + url_path
        return config;
    }

    get_uri_path(url)
    {
        const startUrlPath = url.indexOf("/", "https://".length)
        if (startUrlPath == -1) {
            this.url_path = ""
            // this.url_path_index = config.url.length // utile ?
            // console.log(url)
            return ""
        }
        else
        {
            this.url_path = url.substr(startUrlPath)
            // this.url_path_index = startUrlPath
            return url.substr(startUrlPath)
        }
    }


    change_url()
    {
        console.log("change_url")
        console.log(this.config)

        this.config.url = "https://estcequecestbientotleweekend.fr/";
    }

    intercept_response_success(response) {
        return response
    }
    intercept_response_error(error) {

        console.log(error)
        console.log(error.response.status)
        if (this.unavailable_error(error.response.status))
        {
            this.change_url()
            return this.axios.request(this.config);
        }
        return error;
    }

    unavailable_error(status_code)
    {
        const ServerErrorBadGateway = 502
        const ServerErrorServiceUnavailable = 503
        const ServerErrorInternal = 500
        const ServerErrorGatewayTimeout = 504

        switch (status_code)
        {
            case ServerErrorBadGateway:
            case ServerErrorGatewayTimeout:
            case ServerErrorInternal:
            case ServerErrorServiceUnavailable:
                return true
            default:
                return false
        }
    }
}

const handler = new QarnotSrvHandler(axios, "https://api.dev.qarnot.com")



// console.log(handler)
const ret = axios.get('https://api.dev.qarnot.com', {}).then((value) => {
    console.log(value);
})
/*.catch((e) => {

    console.log("miss");
    console.log(e.undefined)
}
);
*/