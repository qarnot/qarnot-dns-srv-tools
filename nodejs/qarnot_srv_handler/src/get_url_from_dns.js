const restClient = require('./client');

function balanceSeparateArrayByWeight(priorityArray)
{
    let balancedPriorityArray = [];
    while (priorityArray.length > 0) {
        const sum = priorityArray.reduce((accumulator, currentValue) => accumulator + currentValue.weight, 0);
        let accumulator = 0;
        const randValue = Math.floor(Math.random() * sum);
        const index = priorityArray.findIndex(a => {
            accumulator += a.weight;
            return accumulator > randValue;
        });
        balancedPriorityArray = balancedPriorityArray.concat(priorityArray.splice(index, 1));
    }
    return balancedPriorityArray
}

function balanceArrayByWeight(dnsList) {
  let balancedList = [];
  while (dnsList.length > 0)
  {
    const first = dnsList[0];
    const priorityArray = dnsList.filter(a => a.priority == first.priority);
    const balancedPriorityArray = balanceSeparateArrayByWeight(priorityArray);
    balancedList = balancedList.concat(balancedPriorityArray);
    dnsList = dnsList.filter(a => a.priority != first.priority);
  }
  return balancedList;
}

async function getDnsSrv(srvHostname) {
  try {
    const { Resolver } = require('dns').promises;
    const dns = new Resolver();
    let dnsList = await dns.resolveSrv(srvHostname, 'SRV');
    return dnsList;
  } catch (e) {
    return [];
  }
}

async function checkDnsUri(balancedList) {
  for (let index = 0; index < balancedList.length; index++) {
    const url = 'https://' + balancedList[index].name;
    const config = {
      clusterUrl: url,
      clusterUnsafe: true,
    };
    const httpClient = new restClient(config);
    try {
      const response = httpClient.get('/settings');
      await response;
      if (response.responseContent.statusCode >= 200 && response.responseContent.statusCode < 300) {
        return url;
      }
    } catch (e) {
      continue;
    }
  }
  return null;
}

function getQarnotDnsSrvTcpAddress(uri) {
  const isQarnotRegex = new RegExp('https://api\\.(.*\\.)?qarnot\\.com');
  if (isQarnotRegex.test(uri)) {
    // get the dns srv address
    return uri.replace('https://api.', '_api._tcp.');
  }
  return null;
}

function prioritySort(a, b) {
  return a.priority - b.priority;
}

async function get_sort_tcp_list(srvHostname)
{
  // return [{ name: "api.dev.qarnot.com" }, { name: "api.dev.qarnot.com" }, { name:"estcequecestbientotleweekend.fr"}]
  // Get the dns srv list
  const dnsList = await getDnsSrv(srvHostname);

  // sort by priority
  dnsList.sort(prioritySort);

  // balance by weight
  return balanceArrayByWeight(dnsList);
}


async function getUrlFromDnsSrv(uri) {
  // get the dns srv address
  const srvHostname = getQarnotDnsSrvTcpAddress(uri);
  if (srvHostname != null) {
    // Get the dns srv list
    const dnsList = await getDnsSrv(srvHostname);

    // sort by priority
    dnsList.sort(prioritySort);

    // balance by weight
    const balancedList = balanceArrayByWeight(dnsList);

    // Check the get and return it if it is validated
    const response = await checkDnsUri(balancedList);

    if (response != null) {
      return response;
    }
  }
  return uri;
}



// Export for the unit tests
module.exports = {
  balanceArrayByWeight: balanceArrayByWeight,
  getDnsSrv: getDnsSrv,
  checkDnsUri: checkDnsUri,
  getQarnotDnsSrvTcpAddress: getQarnotDnsSrvTcpAddress,
  getUrlFromDnsSrv: getUrlFromDnsSrv,
  prioritySort: prioritySort,
  get_sort_tcp_list: get_sort_tcp_list,
};
