# NounBase: a relational token data service

A lightweight, portable platform, data service library to persist and access nouns by context in .NET Core in an Sqlite file.
![Screenshot](TokenModel.png)

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. 

## Who is this for?

.NET core developers needing a fast, lightweight, dynamic semantic entity token graph persistence and query mechanism. 
It may be useful to applications that don't know in advance the schema or model component relations but need to dynamically capture holonymic/meronymic semantic relations of labeled observations.

### Demonstration
![Example](Example.png)

Reference `NounBase` and obtain the service.
```
var service = services.GetService<ITokenService>();
```

Build a token graph, starting with a null context domain collective noun, Countries, for example:
```
var countries = service.GetToken("Countries" );
```
Add individuals to the collection:
```
var country = service.GetToken("US",countries);
```
Continue to build out other noun relations in the same Collection-Individual pattern:
```
var states = service.GetToken("States", country);
var state = service.GetToken("HI", states);
var cities = service.GetToken("Cities", state);
var city = service.GetToken("Hilo", cities);
```
Subsequent context path result child enumeration with query:
```
var states = service.Get(@"Countries\US\States");
Console.WriteLine((states.Children.Where(x=>x.Token=="HI").Count()==1)? "PASS":"FAIL")
```

## Acknowledgments/Current Projects

Grateful to anyone contributing or using this project (please list it here by being a contributor or opening an issue here with a link to your project).
    -

## Contributing

Please contribute to this project! There is no roadmap per se, but beneficial pull requests will be merged. In general, the goals are to query and persist semantic relations dynamically.

## Authors

**Dan Mayer Sr** - Initial work - ez2rem
See also the list of contributors who participated in this project.

## License

This project is licensed under the GNU GPL License - see the GNU-GPLv3-License.txt file for details

