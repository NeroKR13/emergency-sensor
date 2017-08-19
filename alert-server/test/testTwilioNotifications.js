var expect = require('chai').expect;
var supertest = require('supertest');
var mockery = require('mockery');
var stub = require('sinon').stub;

var app = require('../webapp');
var config = require('../config');

describe('Twilio notifications on error', function() {
  var agent = supertest(app);
  var msgCreateStub;

  before(() => {
    // mockery.deregisterAll();
    mockery.enable({
      useCleanCache: true,
      warnOnReplace: false,
      warnOnUnregistered: false
    });

    msgCreateStub = stub().returns(Promise.resolve({}));

    function TwilioMock() {
      return {
        api: {
          messages: {
            create: msgCreateStub,
          },
        },
      }
    }

    mockery.registerMock('twilio', TwilioMock);
  });

  after(function () {
    mockery.deregisterAll();
    mockery.disable();
  });

  describe('GET /error', function() {
    it('should return an error', function() {
      return agent
        .get('/error')
        .expect(function(res) {
          expect(res.status).to.equal(500);
          expect(msgCreateStub.calledTwice).to.be.true;
        });
      });
  });
});
