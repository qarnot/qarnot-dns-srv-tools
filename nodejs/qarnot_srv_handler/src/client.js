"use strict";
const srv_functions = require('./get_url_from_dns');

const axios = require('axios').default;

class QarnotSrvHandler {
    constructor(axios, url, cache_duration) {
        this.axios = axios; // const
        this.url = url; // const
        this.tcp_url = srv_functions.getQarnotDnsSrvTcpAddress(url);; // const
        // get tcp uri
        this.cache_duration = cache_duration
        this.cache_time_next = Date.now();
        this.find = null
        axios.interceptors.request.use((request) => {
            return this.intercept_request_success(request).then((request) => request).catch((e) => console.log(e))}, (error) => { return this.intercept_request_error(error) })
        axios.interceptors.response.use((response) => { return this.intercept_response_success(response) }, (error) => { return this.intercept_response_error(error) })
    }

    intercept_request_error(error)
    {
        return error
    }

    async intercept_request_success(config)
    {
        // this.config = config
        const url_path = await this.get_uri_path(config.url)
        // console.log(config)
        config = await this.set_new_uri(config, url_path)

        console.log(config.url)
        return config
    }

    async get_new_url(config)
    {
        if (this.cache_time_next < Date.now() && this.tcp_url)
        {
            // console.log("Date.now()")
            // create a new dns call
            this.sort_list = await srv_functions.get_sort_tcp_list(this.tcp_url)
            // console.log(this.sort_list)
            this.cache_time_next = add_minutes(Date.now(), this.cache_duration)
            // console.log(this.cache_time_next)

            if (this.sort_list && this.sort_list.length > 0)
            {
                this.sort_list.forEach(e => e.last_fail = Date.now())
            }
        }

        return this.get_valid_list_uri()
    }

    get_valid_list_uri()
    {
        if (this.find)
        {
            return "https://" + this.find.name
        }
        return this.url
    }

    async set_new_uri(config, url_path)
    {
        const new_url = await this.get_new_url(config)
        if (url_path)
        {
            config.url = new_url + "/" + url_path
        }
        else
        {
            config.url = new_url
        }
        return config;
    }

    get_uri_path(url)
    {
        const startUrlPath = url.indexOf("/", "https://".length)
        if (startUrlPath == -1) {
            return ""
        }
        else
        {
            return url.substr(startUrlPath)
        }
    }

    intercept_response_success(response) {
        return response
    }

    intercept_response_error(error)
    {
        // console.log(error)
        if (this.unavailable_error(error.response.status))
        {
            if (this.find)
            {
                this.find.last_fail = add_minutes(Date.now(), 1)
            }
            if (this.sort_list)
            {
                if (this.sort_list && this.sort_list.length > 0) {
                    this.find = this.sort_list.find(e => e.last_fail < Date.now())
                    console.log(this.sort_list)
                    console.log(this.find)
                }
            }
            return this.axios.request(error.config);
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
            case 401:
                return true
            default:
                return false
        }
    }
}

const handler = new QarnotSrvHandler(axios, "https://api.dev.qarnot.com", 5)

function add_minutes(date_millisecond, minutes)
{
    return date_millisecond + (minutes * 60 * 1000);
}


// console.log(handler)
const ret = axios.get('https://api.dev.qarnot.com', {}).then((value) => {
    // console.log(value);
    console.log(value.status);
})
/*.catch((e) => {

    console.log("miss");
    console.log(e.undefined)
}
);
*/