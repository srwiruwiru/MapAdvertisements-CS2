# CS2-Poor-MapAdvertisements
This plugin allows for server owners to create spray type advertisements that are placed on wall.

## [📌] Setup

## [📝] Configuration
| Option  | Description |
| ------------- | ------------- |
| Admin Flag (string) | Which flag will have access to all of the commands  |
| Vip Flag (string) | Which flag would not see advertisements that are not forced on vip users |
| Props Path (string[]) | Paths for all advertisements that your addon have |
| Custom Position Values (int[]) | Custom values that will change position of the advert |
| Custom Angle Values (int[]) | Custom values that will change rotation of the advert |
| Enable commands (bool) | If you want commands to be enabled. (for example, after you placed all of the advertisements you might not need commands anymore) |
| Debug Mode (bool) | If plugin should log errors, etc |

### [📝] Config example:
```
{
  "Admin Flag": "@css/root",
  "Vip Flag": "@vip/noadv",
  "Props Path": [
    "models/advert1.vmdl",
    "materials/decal_1.vmat",
    "materials/advert_3.vmat",
    "materials/advert_1.vmat"
  ],
  "Custom Position Values": [1,5,10],
  "Custom Angle Values": [1,5,10],
  "Enable commands": true,
  "Debug Mode": true,
  "ConfigVersion": 1
}
```

## [🛡️] Admin commands
Tried to make plugin idiot proof (since I did a lot of mistakes).
| Command  | Description |
| ------------- | ------------- |
| css_mapadverts | Menu that allows to setup advertisements |