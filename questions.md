## Returning Results
### What exactly to return
(e.g. what should I return on register)
### How to return answers the best way?
### How to return JWT?
#### Access Token
#### Refresh Token
Read about storing the refresh token as a claim, and making the refresh-token endpoint authenticated. Is this the right way? If the Access-token is expired, how would the user refresh the token, since he isn't authenticated anymore?

## Database
### Optimizing Database?
### Cold Start
I noticed that the first 1 or 2 requests take **way** longer than usual. For register it was like 3s for the first request in total. The console told me:
```
 Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (213ms) [Parameters=[@__request_RefreshToken_0='?'], CommandType='Text', CommandTimeout='30']
      SELECT u."Id", u."Email", u."PasswordHash", u."RefreshToken", u."RefreshTokenExpiry"
      FROM "Users" AS u
      WHERE u."RefreshToken" = @__request_RefreshToken_0
      LIMIT 1

```
With the second request it's already less, and after the third request it's only 1ms (database)

## Logging
### The right way to log
### Different services, that can help monitoring the logs?

## Code optimization

## Further improvements

