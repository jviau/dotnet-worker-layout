# Build Output

All output from build should go into this folder. Aside from this file, **nothing** should ever be checked in within this folder.

- `out/bin`: all final build output in the form of `out/bin/{AssemblyName}/{Configuration}/{TargetFramework}/`
- `out/obj`: all intermediate build output in the form of `out/obj/{ProjectName}/`
- `out/pkg`: all 'packaged' build output in the form of `out/pkg/{Package}`. This contains all shippable build artifacts.

**note**: All output from `samples` goes into `out/samples/bin` and `out/samples/obj`. This is to keep them separate from shipping output.
