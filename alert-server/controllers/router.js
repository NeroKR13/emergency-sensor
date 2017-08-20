var twilioClient = require('../twilioClient');
var fs = require('fs');
var admins = require('../config/administrators.json');
var JsonDB = require('node-json-db');
var db = new JsonDB("log", true, false);

// Map routes to controller functions
module.exports = function(router) {
  router.get('/error', function(req, resp) {
    throw new Error('Derp. An error occurred.');
  });

  router.post('/alert', function (req, res) {
    db.push("/alert", {
      message:"Alert, Stranger Danger",
      timestamp: new Date()
    });
    admins.forEach(function(admin) {
      var messageToSend = 'ALERT! STRANGER DANGER! :siren: @ ' + new Date();
      twilioClient.sendSms(admin.phoneNumber, messageToSend);
    });
    res.end(200)("Alert posted");
  });
  router.get('/alert', function(req, res) {
    try {
      var data = db.getData("/alert");
      res.status(200).json(data);
    }
    catch(error){
      console.error(error);
      res.status(500).json({})
    }
  });

};
