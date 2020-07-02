"use strict";
const dnsHandler = require('./QarnotSrvHandler');

const axios = require('axios').default;

const handler = new dnsHandler.QarnotSrvHandler(axios, "https://api.dev.qarnot.com", 5)

const ret = axios.get('https://api.dev.qarnot.com', {}).then((value) => {
    console.log(value.status);
})
