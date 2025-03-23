\ pin avail!!!
\ https://github.com/JohnOH/embello/blob/master/explore/1608-forth/flib/stm32f1/io.fs
\ I/O pin primitives

$40010800 constant GPIO-BASE
      $00 constant GPIO.CRL   \ reset $44444444 port Conf Register Low
      $04 constant GPIO.CRH   \ reset $44444444 port Conf Register High
      $08 constant GPIO.IDR   \ RO              Input Data Register
      $0C constant GPIO.ODR   \ reset 0         Output Data Register
      $10 constant GPIO.BSRR  \ reset 0         port Bit Set/Reset Reg
      $14 constant GPIO.BRR   \ reset 0         port Bit Reset Register

: bit ( u -- u )  \ turn a bit position into a single-bit mask
  1 swap lshift  1-foldable ;

: io ( port# pin# -- pin )  \ combine port and pin into single int
  swap 8 lshift or  2-foldable ;
: io# ( pin -- u )  \ convert pin to bit position
  $1F and  1-foldable ;
: io-mask ( pin -- u )  \ convert pin to bit mask
  io# bit  1-foldable ;
: io-port ( pin -- u )  \ convert pin to port number (A=0, B=1, etc)
  8 rshift  1-foldable ;
: io-base ( pin -- addr )  \ convert pin to GPIO base address
  $F00 and 2 lshift GPIO-BASE +  1-foldable ;

: 'f ( -- flags ) token find nip ;

: (io@)  (   pin -- pin* addr )
  dup io-mask swap io-base GPIO.IDR  +   1-foldable ;
: (ioc!) (   pin -- pin* addr )
  dup io-mask swap io-base GPIO.BRR  +   1-foldable ;
: (ios!) (   pin -- pin* addr )
  dup io-mask swap io-base GPIO.BSRR +   1-foldable ;
: (iox!) (   pin -- pin* addr )
  dup io-mask swap io-base GPIO.ODR  +   1-foldable ;
: (io!)  ( f pin -- pin* addr )
  swap 0= $10 and + dup io-mask swap io-base GPIO.BSRR +   2-foldable ;

: io@ ( pin -- f )  \ get pin value (0 or -1)
  (io@)  bit@ exit [ $1000 setflags 2 h, ' (io@)  ,
  'f (io@)  h, ' bit@ , 'f bit@ h, ] ;
: ioc! ( pin -- )  \ clear pin to low
  (ioc!)    ! exit [ $1000 setflags 2 h, ' (ioc!) ,
  'f (ioc!) h, '    ! , 'f    ! h, ] ;
: ios! ( pin -- )  \ set pin to high
  (ios!)    ! exit [ $1000 setflags 2 h, ' (ios!) ,
  'f (ios!) h, '    ! , 'f    ! h, ] ;
: iox! ( pin -- )  \ toggle pin, not interrupt safe
  (iox!) xor! exit [ $1000 setflags 2 h, ' (iox!) ,
  'f (iox!) h, ' xor! , 'f xor! h, ] ;
: io! ( f pin -- )  \ set pin value
  (io!)     ! exit [ $1000 setflags 2 h, ' (io!)  ,
  'f (io!)  h, '    ! , 'f    ! h, ] ;

%0000 constant IMODE-ADC    \ input, analog
%0100 constant IMODE-FLOAT  \ input, floating
%1000 constant IMODE-PULL   \ input, pull-up/down

%0001 constant OMODE-PP     \ output, push-pull
%0101 constant OMODE-OD     \ output, open drain
%1001 constant OMODE-AF-PP  \ alternate function, push-pull
%1101 constant OMODE-AF-OD  \ alternate function, open drain

%01 constant OMODE-SLOW  \ add to OMODE-* for 2 MHz iso 10 MHz drive
%10 constant OMODE-FAST  \ add to OMODE-* for 50 MHz iso 10 MHz drive

: io-mode! ( mode pin -- )  \ set the CNF and MODE bits for a pin
  dup io-base GPIO.CRL + over 8 and shr + >r ( R: crl/crh )
  io# 7 and 4 * ( mode shift )
  $F over lshift not ( mode shift mask )
  r@ @ and -rot lshift or r> ! ;

: io-modes! ( mode pin mask -- )  \ shorthand to config multiple pins of a port
  16 0 do
    i bit over and if
      >r  2dup ( mode pin mode pin R: mask ) $F bic i or io-mode!  r>
    then
  loop 2drop drop ;

: io. ( pin -- )  \ display readable GPIO registers associated with a pin
  cr
    ." PIN " dup io#  dup .  10 < if space then
   ." PORT " dup io-port [char] A + emit
  io-base
  ."   CRL " dup @ hex. 4 +
   ."  CRH " dup @ hex. 4 +
   ."  IDR " dup @ h.4  4 +
  ."   ODR " dup @ h.4 drop ;

0 0  io constant PA0
0 1  io constant PA1
0 2  io constant PA2
0 3  io constant PA3
0 4  io constant PA4
0 5  io constant PA5
0 6  io constant PA6
0 7  io constant PA7
0 8  io constant PA8

PA0 constant LCD_BL
PA2 constant LCD_RS \ 1 data 0 cmd
PA3 constant LCD_EN \ 1 active
PA4 constant LCD_D4
PA5 constant LCD_D5
PA6 constant LCD_D6
PA7 constant LCD_D7

\ don't forget to start the systicks-hz, else it will hang 

\ define Ports

\ Backlight 0 active, connected to K on LCD, but it is optionaly
PA0 constant LCD_BL
\ PA1 constant LCD_RW \ not used
PA2 constant LCD_RS \ 1 data 0 cmd
PA3 constant LCD_EN \ 1 active

\ 4 bit connection
PA4 constant LCD_D4
PA5 constant LCD_D5
PA6 constant LCD_D6
PA7 constant LCD_D7

\ 0 = cursor and blink off, 2 = cursor on , 1 = blink on , 3 = both
1 constant LCD_CB


\ demo char
: h
  8 lshift + 8 lshift + 8 lshift +
;

create bell
hex
04
0E
0E
0E h ,
1F
00
04
00 h ,
FFFF \ end
,
decimal



\ send only upper nibble
: LCD_sendn ( byte - - )
    dup $80 and 0= if LCD_D7 ioc! else LCD_D7 ios! then
    dup $40 and 0= if LCD_D6 ioc! else LCD_D6 ios! then
    dup $20 and 0= if LCD_D5 ioc! else LCD_D5 ios! then
    $10 and 0= if LCD_D4 ioc! else LCD_D4 ios! then
    LCD_EN ios!
    1 ms.delay
    LCD_EN ioc!
;


\ send byte in two nibbles, upper first
: LCD_send ( byte - - )
    dup
    LCD_sendn
    4 lshift \ move lower nibble
    LCD_sendn
    1 ms.delay
;

: LCD_cmd
    LCD_RS ioc! \ switch to cmd mode
    LCD_send
    LCD_RS ios! \ back to data mode
    5 ms.delay
;

: LCD_clear ( - - )
    $01 LCD_cmd
    $06 LCD_cmd \ entry mode left to right, no auto shift
;

: LCD_off
    $08 LCD_cmd
\    [ifdef] LCD_BL
\	LCD_BL ios!
\    [then]
;

\ switches LCD on and BL on
: LCD_on
    $0C LCD_CB or LCD_cmd
\    [ifdef] LCD_BL
\	LCD_BL ioc!
\    [then]
;

\ reset curso pos and view
: LCD_home
    $02 lcd_cmd
;

\ set cursor position 
: LCD_pos ( col row - - )
    6 lshift or $80 or
    LCD_cmd
;

\ moves display view right for positive number, left for negative numbers
: LCD_shift ( chars - - ) 
   dup 0= if exit then
   dup 0< if abs $1C else $18 then
   swap 0 do
       dup lcd_cmd
   loop
   drop
;

\ moves cursor left for positive number, right for negative numbers
: LCD_cshift ( chars - - ) 
   dup 0= if exit then
   dup 0< if abs $10 else $14 then
   swap 0 do
       dup lcd_cmd
   loop
   drop
;

\ writes a string to the display
: LCD_write ( c-addr length - - )
  0 do
    dup c@ LCD_send
    1+
  loop
  drop
;

\ load cgram or ddram for selfmade characters
\ start needs to be between 40 - 78 vor characters or 80 - for direct display
\ note current position 0 0 afterwards
: LCD_ram ( addr start - - )
    dup $40 < if 2drop exit then \ bad addr
    lcd_cmd
    begin
	dup c@
	dup $FF <>
    while
	    lcd_send
	    1+
    repeat
    2drop
    0 0 lcd_pos
;


    




\ initialize the LCD
\ writes ms.delayg in LCD and switches backlight on if nothing is on stack(!)
\ 
: LCD_init ( - - )

    \ initialize Ports
\    [ifdef] LCD_BL
\	OMODE-OD LCD_BL io-mode!
\	LCD_BL ios! \ BL off
\    [then]
    OMODE-OD LCD_RS io-mode!
    LCD_RS ios! \ data mode
\    [ifdef] LCD_RW
\	OMODE-PP LCD_RW \ read or write, not used/always low
\	LCD_RW ioc!
\    [then]
    OMODE-PP LCD_EN io-mode!
    LCD_EN ioc! \ nothing
    
    OMODE-PP LCD_D4 io-mode!
    OMODE-PP LCD_D5 io-mode!
    OMODE-PP LCD_D6 io-mode!
    OMODE-PP LCD_D7 io-mode!
    
    \ set cmd mode
    LCD_RS ioc! 
    
    \ sequenze for 4bit
    50 ms.delay
    $30 LCD_sendn
    5 ms.delay
    $30 LCD_sendn
    1 ms.delay
    $30 LCD_sendn
    10 ms.delay
    $20 LCD_sendn \ 4bit mode
    10 ms.delay 
    
    \ display init
    $28 LCD_send \ 4bit mode, char size, disp size
    1 ms.delay
    $08 LCD_send \ display off
    1 ms.delay
    \ reset to data mode reseted in LCD_on
    LCD_clear
    
    \ you don't have to show this notice
    \ comment out if you like, it is just here as demo
    \ if something is on stack don't do that and don't switch Backlight on!
    depth 0= if 
	s" LCD1602 via HD44780 on 4bit interface" LCD_write
	0 1 LCD_pos
	s" writen by SMb (c) 2020" LCD_write
	
	\ LCD and Backlight on
	LCD_on
	
	\ show and scroll a bit
	1000 ms.delay
	21 0 do
	    500 ms.delay
	    1 lcd_shift
	loop
	1000 ms.delay
    then

    \ cleanup again
    LCD_clear
;


\ example
 : LCD_hello
    s" Hello World!"
    LCD_write
 ;
