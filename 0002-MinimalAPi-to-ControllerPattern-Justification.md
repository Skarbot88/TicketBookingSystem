# Web API justification 

My initial thought was to use a minimal API because the requirement was just 3 endpoints and ultimately the overhead was more effort to implement a controller pattern however when writing out the map lambda I noticed I was doing error handling via switch inside the lambda itself and it just didn't read well as it got bigger. If this code was for my eyes only then thats not a problem but the instant I share it, it has to be readable for that person too. 

Along with controllers you get model validation (with less effort), nicer swagger docs (I chose to include annotations) and more personalised logging.