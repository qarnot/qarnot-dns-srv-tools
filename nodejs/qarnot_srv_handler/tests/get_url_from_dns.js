const srv = require('../lib/get_url_from_dns');
const assert = require('chai').assert;

describe('Get url function', function() {
  it('test returnQarnotDnsSrvTcpAddress: https://api.qarnot.com return _api._tcp.qarnot.com', function() {
    assert.equal(srv.returnQarnotDnsSrvTcpAddress('https://api.qarnot.com'), '_api._tcp.qarnot.com');
  });
  it('test returnQarnotDnsSrvTcpAddress: https://api.qualif.qarnot.com return _api._tcp.qualif.qarnot.com', function() {
    assert.equal(srv.returnQarnotDnsSrvTcpAddress('https://api.qualif.qarnot.com'), '_api._tcp.qualif.qarnot.com');
  });
  it('test returnQarnotDnsSrvTcpAddress: https://api.dev.qarnot.com return _api._tcp.dev.qarnot.com', function() {
    assert.equal(srv.returnQarnotDnsSrvTcpAddress('https://api.dev.qarnot.com'), '_api._tcp.dev.qarnot.com');
  });
  it('test returnQarnotDnsSrvTcpAddress: https://apitest.qarnot.com return null', function() {
    assert.equal(srv.returnQarnotDnsSrvTcpAddress('https://apitest.qarnot.com'), null);
  });
  it('test returnQarnotDnsSrvTcpAddress: https://api.testqarnot.com return null', function() {
    assert.equal(srv.returnQarnotDnsSrvTcpAddress('https://api.testqarnot.com'), null);
  });
  it('test returnQarnotDnsSrvTcpAddress: https://apiqarnot.com return null', function() {
    assert.equal(srv.returnQarnotDnsSrvTcpAddress('https://apiqarnot.com'), null);
  });
  it('test returnQarnotDnsSrvTcpAddress: https://api.hello.com return null', function() {
    assert.equal(srv.returnQarnotDnsSrvTcpAddress('https://api.hello.com'), null);
  });
  it('test returnQarnotDnsSrvTcpAddress: https://bob.qarnot.com return null', function() {
    assert.equal(srv.returnQarnotDnsSrvTcpAddress('https://bob.qarnot.com'), null);
  });

  it('Test sort priority', function() {
    const srv_list = [
      { name: '', port: 430, priority: 4, weight: 10 },
      { name: '', port: 430, priority: 1, weight: 10 },
      { name: '', port: 430, priority: 3, weight: 10 },
      { name: '', port: 430, priority: 5, weight: 10 },
      { name: '', port: 430, priority: 2, weight: 10 },
    ];
    srv_list.sort(srv.prioritySort);
    assert.equal(1, srv_list[0].priority);
    assert.equal(2, srv_list[1].priority);
    assert.equal(3, srv_list[2].priority);
    assert.equal(4, srv_list[3].priority);
    assert.equal(5, srv_list[4].priority);
  });

  it('Test balanceArrayByWeight', function() {
    const weightList = [1, 10, 30, 50, 20];
    const testRecurrence = 1000;
    const testPercentMarge = 0.1;
    const testSumWeight = weightList.reduce((a, b) => a + b, 0);
    const weightRecurrence = weightList.map(() => 0);

    const srv_list = weightList.map(weight => {
      return { name: '', port: 430, priority: 2, weight: weight };
    });

    for (let index = 0; index < testRecurrence * testSumWeight; index++) {
      const list = srv.balanceArrayByWeight(srv_list);
      // test the first element
      const index = srv_list.findIndex(a => list[0].weight == a.weight);
      weightRecurrence[index] += 1;
    }

    for (let index = 0; index < weightList.length; index++) {
      const element = weightList[index] * testRecurrence;
      const min = element - element * testPercentMarge;
      const max = element + element * testPercentMarge;
      assert.isOk(
        min <= weightRecurrence[index],
        `balance should be upper than ${min}(${element} +-${100 * testPercentMarge}%) but ${weightRecurrence[index]} find.`
      );
      assert.isOk(
        weightRecurrence[index] <= max,
        `balance should be lower than ${min}(${element} +-${100 * testPercentMarge}%) but ${weightRecurrence[index]} find.`
      );
    }
  });
});
