var twilioClient = require('../twilioClient');
var fs = require('fs');
var admins = require('../config/administrators.json');

// Map routes to controller functions
module.exports = function(router) {
  router.get('/error', function(req, resp) {
    throw new Error('Derp. An error occurred.');
  });

  router.post('/alert', function (req, res) {
    admins.forEach(function(admin) {
      var messageToSend = 'ALERT! STRANGER DANGER! :siren:';
      twilioClient.sendSms(admin.phoneNumber, messageToSend);
    });
  });
};
