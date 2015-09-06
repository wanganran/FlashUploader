# FlashUploader
A prototype of HTML5 uploader that avoids duplicate transmission

This work check the MD5 of a chosen file at the client using Javascipt, and find duplicates at the server. If found, the upload transmission doesn't need to be taken, thus saving upload time and network bandwidth.

For efficiency, the client first pass the file size to the server. If the server doesn't find a same one, the MD5 check then doesn't need to be carried out. 

This is a prototype done at the first Glodon coding competition, within about 20 hours. I finally won the first prize.

This is also a practice of Bootstrap and a revision of Javascript. I haven't touched Javascript for years.

The backend is implemented using an old technique, WebService, inside ASP.NET. I initially intend to use Lift or Play Framework on Scala, but time is limited so I have to use a more proficient one.
