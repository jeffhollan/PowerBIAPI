# Power BI API App
[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

This API App will allow you to authenticate against the Power BI REST API and add rows to an already existing dataset.

## Prerequisites ##

Because this API requires Authentication, and Power BI is not one of the APIs supported out-of-the-box by API Apps Gateway, the app itself will manage authentication and tokens.  Because of this, it is necessary to have a registered application with Azure Active Directory to authorize the calls.

You can register your web application via Azure.  Be sure to grant delegate permissions to the Power BI Service.

When you provision this app you will give it a site name.  You will need this in order to register your application for the redirect URL.  If you registered this API as `PowerBIAPI9a2kd892` your redirect URL would be `https://PowerBIAPI9a2kd892.azurewebsites.net/redirect`.

This app also requires a dataset has been created already via the Power BI REST API.  You will need the dataset ID to use the API App.  You can see documentation on this API and create via the interactive console [here](http://docs.powerbi.apiary.io/#reference/datasets/datasets-collection/create-a-dataset)

## Deploying ##

This API App will deploy a Resource Group, Gateway, API App Host, and API App.  Click the "Deploy To Azure" button above to deploy.  You will need the Client ID and Client Secret from the application you registered with Azure Active Directory.

### Authorizing ###

After deploying, navigate to the API App URL and click the "Authorize" button.  If your Client ID, Secret, and the registered Redirect URL are all correct you should get back a `Successfully Authorized` response.  The API App will manage refresh tokens, so **be sure to change Application Settings for the API App to 'Internal'** after Authentication.  If not, you will have an open endpoint to your Power BI data.

If you are getting errors, you can use the `/showredirect` URL on your API App to have it return the redirect URL it has registered based on deployment URL.  Make sure this matches the redirect URL in your Microsoft Developer Account registration.

## Triggers and Actions ##

### Triggers ###

This API App currently has no triggers.

### Actions ###

This API has two actions:

| Name | Description |
| --- | -----|
| Add Rows (string) | Add rows to the dataset at the table specified.  The rows can be formatted either `{"foo": "bar"}, {"foo": "baz"}` or within array brackets `[ {"foo": "bar"} ]` |
| Add Rows (array) | Same as above, but accepts an array instead of a string.  Useful when passing in arrays from other connectors (e.g. SQL) |
