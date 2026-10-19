from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import math,shutil
BASE=Path(__file__).resolve().parents[1]
OUT=BASE/'docs'/'diagrams'
FONT=r'C:\Windows\Fonts\arial.ttf';COLOR='#25323a'
class Canvas:
 def __init__(self,w,h):self.im=Image.new('RGB',(w,h),'white');self.d=ImageDraw.Draw(self.im)
 def text(self,x,y,s,size=38,center=False):
  font=ImageFont.truetype(FONT,size);self.d.multiline_text((x,y),s,font=font,fill=COLOR,anchor='mm' if center else 'la',align='center' if center else 'left',spacing=10)
 def line(self,pts,width=3):self.d.line(pts,fill=COLOR,width=width,joint='curve')
 def box(self,x,y,w,h,title,lines=(),methods=()):
  self.d.rectangle((x,y,x+w,y+h),fill='white',outline=COLOR,width=3);self.d.rectangle((x,y,x+w,y+65),fill='#edf1f3',outline=COLOR,width=3);self.text(x+w/2,y+33,title,36,True)
  yy=y+86
  for t in lines:self.text(x+20,yy,t,30);yy+=44
  if methods:
   self.line([(x,yy+2),(x+w,yy+2)]);yy+=20
   for t in methods:self.text(x+20,yy,t,30);yy+=44
 def component(self,x,y,w,h,title,detail=''):
  self.d.rectangle((x,y,x+w,y+h),fill='#f3f5f6',outline=COLOR,width=4)
  self.d.rectangle((x+w-60,y+20,x+w-25,y+55),fill='white',outline=COLOR,width=3)
  self.d.rectangle((x+w-72,y+26,x+w-53,y+36),fill='white',outline=COLOR,width=2);self.d.rectangle((x+w-72,y+42,x+w-53,y+52),fill='white',outline=COLOR,width=2)
  self.text(x+w/2,y+h/2-8,title,39,True)
  if detail:self.text(x+w/2,y+h-33,detail,31,True)
 def arrow(self,pts):
  self.line(pts);a,b=pts[-2:];ang=math.atan2(b[1]-a[1],b[0]-a[0]);tri=[b,(b[0]-18*math.cos(ang-.4),b[1]-18*math.sin(ang-.4)),(b[0]-18*math.cos(ang+.4),b[1]-18*math.sin(ang+.4))];self.d.polygon(tri,fill=COLOR)
 def diamond(self,x,y,filled=True):self.d.polygon([(x,y),(x-20,y-12),(x-40,y),(x-20,y+12)],fill=COLOR if filled else 'white',outline=COLOR,width=3)

c=Canvas(1800,1040)
c.component(60,70,500,155,'Мобільний клієнт')
c.component(700,70,540,155,'Кінцева точка поїздок')
c.arrow([(560,148),(700,148)]);c.text(630,38,'HTTPS / JSON',27,True)
c.component(700,350,540,180,'Завершення поїздки','обробник сценарію')
c.arrow([(970,225),(970,350)]);c.text(1090,280,'викликає',32,True)
c.component(60,820,700,155,'Сховище поїздок','реалізує ITripRepository')
c.component(1040,820,700,155,'Сервіс сповіщень','реалізує ITripNotificationPort')
for x,endx,label in [(820,410,'ITripRepository'),(1120,1390,'ITripNotificationPort')]:
 c.line([(x,530),(x,610),(endx,610),(endx,690)])
 c.d.arc((endx-24,690,endx+24,738),180,360,fill=COLOR,width=4)
 c.d.ellipse((endx-14,705,endx+14,733),outline=COLOR,width=4)
 c.line([(endx,733),(endx,820)])
 c.text(520 if endx==410 else 1420,575,label,36,True)
c.text(900,1010,'Півколо: потрібний інтерфейс. Коло: наданий інтерфейс.',30,True)
c.im.save(OUT/'component_trip_api.png')

c=Canvas(1800,1420)
c.box(40,40,480,300,'Пасажир',['- ідентифікатор: UUID','- ім’я: текст','- телефон: текст'],['+ замовити(): Поїздка'])
c.box(660,40,480,300,'Водій',['- ідентифікатор: UUID','- ім’я: текст','- доступний: логічне'],['+ змінитиДоступність()'])
c.box(1280,40,480,300,'Транспортний засіб',['- ідентифікатор: UUID','- номернийЗнак: текст','- місткість: ціле'])
c.box(660,510,480,430,'Поїздка',['- ідентифікатор: UUID','- стан: СтанПоїздки','- вартість: число'],['+ призначити(id: UUID)','+ почати()','+ завершити()','+ скасувати()'])
c.box(40,570,480,225,'Географічна точка',['- широта: число','- довгота: число'])
c.box(1280,570,480,300,'Оплата',['- ідентифікатор: UUID','- сума: число','- стан: СтанОплати'],['+ позначитиСплату()'])
c.box(660,1100,480,300,'Повідомлення про подію',['- ідентифікатор: UUID','- часПодії: дата й час','- опрацьовано: логічне'],['+ опрацювати()'])
# Associations use orthogonal lines, with multiplicities next to the corresponding class.
c.line([(280,340),(280,430),(780,430),(780,510)]);c.text(300,365,'1',30);c.text(530,402,'замовляє',30,True);c.text(790,468,'0..*',30)
c.line([(1010,340),(1010,510)]);c.text(1025,363,'0..1',30);c.text(1120,420,'приймає',28,True);c.text(1028,468,'0..*',30)
c.line([(1140,180),(1280,180)]);c.diamond(1180,180,False);c.text(1208,130,'використовує',20,True);c.text(1150,205,'1',28);c.text(1210,205,'0..*',28)
c.line([(520,680),(660,680)]);c.diamond(660,680,True);c.text(588,632,'маршрут',28,True);c.text(532,702,'2',28);c.text(625,702,'1',28)
c.line([(1140,710),(1280,710)]);c.text(1210,664,'оплата',28,True);c.text(1150,735,'1',28);c.text(1207,735,'0..1',28)
c.line([(900,940),(900,1100)]);c.text(920,952,'1',30);c.text(1030,997,'події безпеки',28,True);c.text(920,1053,'0..*',30)
c.im.save(OUT/'domain_classes.png')
print('Saved both diagrams',OUT)
