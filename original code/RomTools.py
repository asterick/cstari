class DupeCheck:
    def __init__( self, GUI, directory ):
        self.directory = directory
        self.GUI = GUI
        self.UpdateProfile = {
            "size":(192,192,256,64),
            "icon":Chemistry.DefaultIcon,
            "tray":[],
            "title":"Duplicate Finder",
            "atoms":[
                Chemistry.AtomLable((50,4),"Locating Duplicates")
                ]
            }

        self.dupeList = Chemistry.AtomList((0,0,255-16,95),[])
        self.dupeSets = Chemistry.AtomLable((8,231-118-10),"")
        self.DupeManager = {
            "size":(192,148,256,160),
            "icon":Chemistry.DefaultIcon,
            "tray":["iconify","close"],
            "title":"Duplicate Finder",
            "atoms":[
                self.dupeSets,
                Chemistry.AtomButton((256-64-16-1,231-16-20-96,64,24),"Next",self.nextSet),
                Chemistry.AtomButton((256-64-68-16-1,231-16-20-96,64,24),"Remove",self.remove),
                self.dupeList
                ]
            }

        GUI.register( self.UpdateProfile, visible = Chemistry.False )
        GUI.register( self.DupeManager, visible = Chemistry.False )
    def LocateDupes( self ):
        thread.start_new_thread( self.SearchThread, () )
    def remove( self ):
        index = self.dupeList.get()
        os.remove( os.path.join( self.directory, self.list[ index ] ) )
        del self.list[ index ]
        self.dupeList.set( 0 )
        if len(self.list) > 1:
            self.dupeList.set( self.list )
        else:
            self.nextSet()
    def nextSet( self ):
        if len(self.duplicates) == 0:
            self.GUI.hide( Chemistry.True, self.DupeManager )
        else:
            self.list = self.duplicates[0]
            self.dupeList.set( self.list )
            del self.duplicates[0]
            self.dupeSets.set( str(len(self.duplicates)) + " sets" )
    def SearchThread( self ):
        crcDict = {}
        duplicates = []
        self.GUI.hide( Chemistry.False, self.UpdateProfile )
        for file in os.listdir(self.directory):
            try:
                handle = open( os.path.join( self.directory, file ), "rb" )
            except IOError:
                continue
            contents = handle.read()
            handle.close()
            crc = zlib.crc32(contents)
            if crcDict.has_key(crc):
                crcDict[crc].append(file)
            else:
                crcDict.update({crc:[file]})
        self.duplicates = []
        for key in crcDict:
            if len(crcDict[key]) > 1:
                self.duplicates.append( crcDict[key] )
        self.GUI.hide( Chemistry.True, self.UpdateProfile )
        self.GUI.hide( Chemistry.False, self.DupeManager )
        self.nextSet()
