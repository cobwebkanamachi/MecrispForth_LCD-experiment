# MecrispForth_LCD-experiment
clone of https://github.com/SvenMb/MecrispForth_LCD and mixture of https://github.com/JohnOH/embello/blob/master/explore/1608-forth/flib/stm32f1/io.fs
This is an implementation and experiment at my hand.
working:<BR>
<img src="https://github.com/cobwebkanamachi/MecrispForth_LCD-experiment/blob/main/worked-lcd1.jpg"><BR>
<img src="https://github.com/cobwebkanamachi/MecrispForth_LCD-experiment/blob/main/worked-lcd2.jpg"><BR>
<img src="https://github.com/cobwebkanamachi/MecrispForth_LCD-experiment/blob/main/worked1.png"><BR>
<img src="https://github.com/cobwebkanamachi/MecrispForth_LCD-experiment/blob/main/worked2.png"><BR>
<img src="https://github.com/cobwebkanamachi/MecrispForth_LCD-experiment/blob/main/worked3.png"><BR>
<PRE>
invoke bluepill with "Bluepill Diagnostics" of https://mecrisp-stellaris-folkdoc.sourceforge.io/bluepill-diagnostics-v1.6.html
firmware was taken from https://sourceforge.net/projects/mecrisp-stellaris-folkdoc/files/bluepill-diagnostics-v1.6.zip.
invoke teraterm on your pc.
connect with serial baud rate 115200, terminal receive Auto.
then type q into teraterm
type 1 1 + .
ok returned, you could paste lcd.fs all lines.
wait paste all done, then type these 
LCD_init
LCD_hello
Voila, you would see strings on LCD!

Notice on https://github.com/SvenMb/MecrispForth_LCD
if you use same LCD i used (https://akizukidenshi.com/catalog/g/g100038/), original wiring changed at one point only.
LCD's R/W pin (pin is 5) would be connect to GND. If you did not this, LCD display nothing.

Enjoy!
</PRE>
