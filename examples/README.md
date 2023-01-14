# [centipede-shields](centipede-shields)
The `centipede-shields` project contains an example fisob. Fisobs represent non-creature items like spears, rocks, and jellyfish. They typically consist of three or more classes:
- One `PhysicalObject` class (CentiShield.cs) that defines the loaded object's behavior
- One `AbstractPhysicalObject` class (CentiShieldAbstract.cs) that defines the object's save data
- One `Fisob` class (CentiShieldFisob.cs) that defines sandbox unlocks, how the object is parsed from save data, how it interacts with scavs and players, how its icon is rendered, and similar "metadata"

# [mosquitoes](mosquitoes)
The `mosquitoes` project contains an example critob. Critobs are analogous to fisobs but represent custom creatures instead. They consist of:
- One `Creature` class (Mosquito.cs)
- One `Critob` class (MosquitoCritob.cs)
- One `ArtificialIntelligence` class (MosquitoAI.cs) **iff** the creature template's `AI` field is `true`
- One `GraphicsModule` class (MosquitoGraphics.cs) if you use `Creature.InitiateGraphicsModule()`

Note that critobs are designed for creating **unique** creatures. Custom lizards, vultures, cicadas, etc are beyond a critob's scope. That is likely to come in the future though!

# [ElectricSpear](https://github.com/LeeMoriya/ElectricSpear) by LeeMoriya
An updated version of Shiro_PB's original Electric Spear mod that works with BepInEx. Electric Spears are a spear-like weapon that can hold three charges. They recharge when they hit a centipede and discharge when they hit any other creature.

Note as of March 24, 2022: ElectricSpear uses Fisobs v1, so you may not see some parts of Fisobs v2 in there.
