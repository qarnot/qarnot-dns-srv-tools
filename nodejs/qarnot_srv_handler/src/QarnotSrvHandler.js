"use strict";
const srv_functions = require('./get_url_from_dns');

class QarnotSrvHandler {
    constructor(axios, url, cache_duration) {
        // const
        this.axios = axios;
        this.url = url;
        this.tcp_url = srv_functions.getQarnotDnsSrvTcpAddress(url);
        this.cache_duration = cache_duration
        this.unavailable_duration = 5
        this.all_unavailable_duration = 1
        // var
        this.cache_time_next = Date.now();
        this.find = null
        this.sort_list = null
        // axios update
        axios.interceptors.request.use((request) => {
            return this.intercept_request_success(request).then((request) => request)
        }, (error) => { return this.intercept_request_error(error) })
        axios.interceptors.response.use((response) => { return this.intercept_response_success(response) }, (error) => { return this.intercept_response_error(error).then(error) })
    }

    intercept_request_error(error) {
        return error
    }

    async intercept_request_success(config) {
        const url_path = this.get_uri_path(config.url)
        config.url = await this.set_new_uri(config, url_path)
        console.log(config.url)

        return config
    }

    async update_url() {
        if (this.tcp_url && this.cache_time_next < Date.now()) {
            // create a new dns call
            this.sort_list = await srv_functions.get_sort_tcp_list(this.tcp_url)
            this.cache_time_next = add_minutes(Date.now(), this.cache_duration)
            this.find = null

            if (this.sort_list && this.sort_list.length > 0) {
                this.sort_list.forEach(e => e.last_fail = Date.now())
                this.find = this.sort_list[0]
            }
        }
    }

    get_valid_list_uri() {
        if (this.find) {
            return "https://" + this.find.name
        }
        return this.url
    }

    async set_new_uri(config, url_path) {
        await this.update_url()
        const new_url = this.get_valid_list_uri()
        if (url_path) {
            return new_url + "/" + url_path
        } else {
            return new_url
        }
    }

    get_uri_path(url) {
        const startUrlPath = url.indexOf("/", "https://".length)
        if (startUrlPath == -1) {
            return ""
        }
        else {
            return url.substr(startUrlPath + 1)
        }
    }

    intercept_response_success(response) {
        return response
    }

    async intercept_response_error(error) {
        if (this.unavailable_error(error.response.status)) {
            if (this.find) {
                this.find.last_fail = add_minutes(Date.now(), this.unavailable_duration)
            }
            if (this.sort_list && this.sort_list.length > 0) {
                this.find = this.sort_list.find(e => e.last_fail < Date.now())
                if (!this.find) {
                    await minute_sleep(this.all_unavailable_duration)
                    await this.update_url()
                    return await this.intercept_response_error(error)
                }
            }
            return this.axios.request(error.config);
        }
        return error;
    }

    unavailable_error(status_code) {
        const ServerErrorBadGateway = 502
        const ServerErrorServiceUnavailable = 503
        const ServerErrorInternal = 500
        const ServerErrorGatewayTimeout = 504

        switch (status_code) {
            case ServerErrorBadGateway:
            case ServerErrorGatewayTimeout:
            case ServerErrorInternal:
            case ServerErrorServiceUnavailable:
            case 401:
            case 404:
                return true
            default:
                return false
        }
    }
}


function add_minutes(date_millisecond, minutes) {
    return date_millisecond + (minutes * 60 * 1000);
}

function minute_sleep(min) {
    return sleep(min * 1000 * 60)
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

module.exports = {
    QarnotSrvHandler: QarnotSrvHandler,
}