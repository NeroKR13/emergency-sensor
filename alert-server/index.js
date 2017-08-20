var http = require('http');
var config = require('./config');

// Create Express web app
var app = require('./webapp');

// Create an HTTP server and listen on the configured port
var server = http.createServer(app);
var port = config.port || process.env.port || 3000;
server.listen(port, function() {
  console.log('Express server listening on *:' + port);
});
