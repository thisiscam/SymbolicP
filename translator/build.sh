# install the nuget.exe from p-org (other version doesn't work for some reason)
wget https://raw.githubusercontent.com/p-org/P/master/Bld/nuget.exe /path/to/nuget

PROJ_ROOT = $(pwd)

# submodule update
cd $PROJ_ROOT
git submodule update --init --recursive

## Build BDD stuff

# build buddy BDD
cd $PROJ_ROOT/translator/libraries/BuDDySharp
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
make

# build sylvan BDD
cd $PROJ_ROOT/translator/libraries/BuDDySharp
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
make

# build Wrapper library used by Multise runtime
cd $PROJ_ROOT/translator/runtimes/multise_csharp/BDDToZ3Wrap
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
make
cd ..
xbuild /property:Configuration=Release /property:Platform=x64 # there will probably be two errors, but ignore them for now

## build C# code instrumentor(for converting to multise-injected code)
cd translator/translators/multise_csharp_translator/MutliSETransformer
mono /path/to/nuget restore
xbuild

## install python packages
cd $PROJ_ROOT/translator
virtualenv env
source env/bin/active # do this everytime you want to use the translator
pip install -r requirements.txt
deactivate # do this when you are done with the translator

# build one test case
cd $PROJ_ROOT/translator
virtualenv env
python -m scripts.translate -m multise_csharp tests/two-phase-commit.p
deactivate
cd two-phase-commit
mono /path/to/nuget restore
xbuild /property:Configuration=Release /property:Platform=x64 # there will probably be two errors, but ignore them for now
mono /bin/Release/two-phase-commit.exe
