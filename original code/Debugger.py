################################################################################
##
##  PYTARI - Atari 2600 emulation in python
##
##  Copyright (c) 2000-2005 by Bryon Vandiver
##
##  See the file "license" for information on usage and redistribution of
##  this file, and for a DISCLAIMER OF ALL WARRANTIES.
##
################################################################################
##
##   This is the debugger module, containing some chemistry dialogs
##   that can be used to do various things with the atari emulator,
##   generally used for bug testing
##
################################################################################

import CPUops, Chemistry, Numeric, RandomArray, pygame, TIA
from pygame.locals import *
from string import *
from types import *

class AtomUpdater:
    """This class is used to update a dialog on repaint"""
    def __init__( self, callback ):
        self.callback = callback
    def showFocus( self ):
        return False
    def get( self ):
        return self.callback
    def set( self, callback ):
        self.callback = callback
    def dim( self ):
        return 0,0,1,1
    def inherit( self, font, fore, selected, back, icons ):
        pass
    def paint( self, surface, focus = False ):
        self.callback()
    def event( self, event, pos = None ):
        pass

class RamViewer:
    def __init__( self, GUI, RAM, font, Registers = None ):
        self.containerClass = Chemistry.AtomContainer((0,0,374+16,208),[])
        scrollClass = Chemistry.AtomScroll((362+32,0,-160),(0,0),self.ScrollChanged)

        if type(RAM) is not ListType:
            RAM = [RAM]
        
        self.GUI = GUI
        self.Registers = Registers
        self.RAM = RAM
  
        self.View = {
            "size":(32,32,395+32,208+36),
            "icon":None,
            "title":"Memory",
            "tray":["iconify", "close"],
            "atoms":[
                AtomUpdater(self.Update),
                self.containerClass,
                scrollClass
                ]
            }

        
        GUI.register( self.View )

        x, y = 48, -4
        for setIndex in range(len(RAM)):
            RamSet = RAM[setIndex]
            name, contents = RamSet
            lableSet = []
            RAM[setIndex] = (Numeric.array(contents), contents, lableSet)
            
            if x != 48:
                x = 48, y + 20
            lable = Chemistry.AtomLable((32,y), name, color = (255,255,100), font = font)
            self.containerClass.set(lable)
            y = y + 20
            for index in range(len(contents)):
                if (index % 16) == 0:
                    lable = Chemistry.AtomLable((8,y),"%02X" % index,color = (255,220,150), font = font)
                    self.containerClass.set(lable)
                if index % 2:
                    color = (0,255,255)
                else:
                    color = (255,255,255)
                lable = Chemistry.AtomLable((x,y),"%02X" % (contents[index]),color = color, font = font)
                self.containerClass.set(lable)
                lableSet.append(lable)
                if (index % 16) == 15:
                    x, y = 48, y + 20
                else:
                    x = x + 20
        if y > 216 - 16:
            scrollClass.set((0,y - 216 + 16))
        else:
            self.View["atoms"].remove(scrollClass)
            self.View["size"] = (32,32,395+10,208+36)
        self.containerClass.set((0,0))

    def Unload( self ):
        self.GUI.unregister( self.View )
        
    def Show( self ):
        self.GUI.hide( Chemistry.False, self.dialog )

    def ScrollChanged( self, value ):
        self.containerClass.set((0,-value))

    def Update( self ):
        for RamSet in self.RAM:
            old, new, lableSet = RamSet
            for index in Numeric.nonzero(old ^ new):
                old[index] = new[index]
                lableSet[index].set("%02X" % (new[index]))

#
#   TIA Register viewer class
#
#   Needs:
#       Player Size (repeats, gaps, stretching)
#       Sound registers (volume, form, freq)
#

class TIARegisters:
    def Update( self ):        
        self.ScanLine.set_palette( self.tia.colors )
        scanline = pygame.surfarray.pixels2d( self.ScanLine )
        scanline[136:,0] = Numeric.repeat(self.tia.LastScanLine[:160],2).astype("b")
        scanline[136:,1] = Numeric.repeat(self.tia.LastScanLine[:160],2).astype("b")
        scanline[136:,2] = Numeric.repeat(self.tia.ScanLine[:160],2).astype("b")
        scanline[136:,3] = Numeric.repeat(self.tia.ScanLine[:160],2).astype("b")

        PF = pygame.surfarray.pixels2d( self.GRPF )
        P0 = pygame.surfarray.pixels2d( self.GRP0 )
        P1 = pygame.surfarray.pixels2d( self.GRP1 )
        P0A = pygame.surfarray.pixels2d( self.GRP0A )
        P1A = pygame.surfarray.pixels2d( self.GRP1A )

        for slot in self.collisionMap:
            mask, x, y = slot
            if self.tia.CollisionMap[mask]:
                self.matrix.blit( self.gui.icons, (x,y), (112,48,16,16))
            else:
                self.matrix.blit( self.gui.icons, (x,y), (96,48,16,16))                            

        playfield           = Numeric.zeros((20))
        playfield[0:4]      = TIA.BitMaps[self.tia.PF0Grp][3::-1]
        playfield[4:12]     = TIA.BitMaps[self.tia.PF1Grp]
        playfield[12:]      = TIA.BitMaps[self.tia.PF2Grp][::-1]
        playfield           = Numeric.repeat( playfield, 16 ).astype("b")

        player0             = Numeric.repeat( TIA.BitMaps[self.tia.P0Grp], 16 ).astype("b")
        player0d            = Numeric.repeat( TIA.BitMaps[self.tia.P0Grp], 16 ).astype("b")
        player1             = Numeric.repeat( TIA.BitMaps[self.tia.P1GrpDel], 16 ).astype("b")
        player1d            = Numeric.repeat( TIA.BitMaps[self.tia.P1GrpDel], 16 ).astype("b")

        for x in range(16):
            PF[:,x] = playfield 
            P0[:,x] = player0
            P1[:,x] = player1
            P0A[:,x] = player0d
            P1A[:,x] = player1d
