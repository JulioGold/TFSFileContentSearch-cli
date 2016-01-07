# TFSFileContentSearch-CLI  
TFS search inside file content.  
  
## Usage  

```  
TFSFileContentSearch-CLI.exe -server="http://tfsserver/DefaultCollection" -path="$/Project/Main/Source/Portal/Controllers/Api" -searchpatterns="static void Main(string[] args),public class" -filepatterns="*.cs,*.config" -v
```  
  
* s|server = Server address with default collection. Ex: http://tfsserver/DefaultCollection  
* p|path = Path to especific location on TFS where you want to find. Ex: $/Project/Main/Source/Portal/Controllers/Api  
* sp|searchpatterns = Search patterns.  
* fp|filepatterns = File paterns will filter the files where the search will do.  
* v|verbose = Show verbosity.  
* h|help = Show this message and exit.  
  
OBS: Use it carefully. Depending wath you specify on the path, the query may take a long time to be completed.  

Thanks  
