   M S B A M L   `   `   `  ����  MTG Library2    MTG_Library2.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  MTG Card Viewer��5   	   $	��600��6!   $	��800��6.    � 	Resources 5          BooleanToVisibilityConverter �� 
TargetType �� Property �� Value �� 
TargetType ��Name J� IsHitTestVisible �� RelativeSource	 �� 	Converter��%�  &	        '��      7   �� 5   
   6'   �� 5   
   " ��6   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� $ PART_ContentHost��6+   �� 5      $��Search for cards...��6(   $ False��5   (   $��Center=�5   (   $
��Left=�5   (   
�������$��Italic$�5   (   y�5   (   �� *Text.IsEmpty+ �� *TemplatedParent+8	   5      5      5       5!      5"      .��5$      � .+�5&   
   �� -   5        
 ��Name$
 	SearchBar��5&      $��10q�6%   $	��Top=�61   $��Stretch=�6I   5'      �� 5)      -   5         ��Name$ LoadingIndicator��5)      $��
Loading...��5*      $��Center=�5+      $��Center=�5,      $y�	Collapsed=�5-      $��20(�5.      �� 50   
   -   5         ��Name$ ResultsListView��50      $��10,50,10,50q�6+   $��Stretch=�6@   $��Stretch=�6\   ��51      �� 52      .R�53      ��  �� Orientation$ 
Horizontal=�6!   $��5q�6:   .+�55      �� $	��100��6>   $	��150��6J   $��5q�6W   ��6    �� *
image_uris+6W   �� 58      .+�59      �� $
��Bold"�6>   $��16(�6P   ��6(   �� *name+6P   �� 5:      $��14(�6=   
���������6(   �� *set+6K   �� 5;      $��Collector Number: ��6(   $
��Bold"�6B   
�������6T   �� 5<      ��6(   �� *collector_number+�� 5=      $��
Quantity: ��6(   $
��Bold"�6:   
�������6L   �� 5>      ��6(   �� *
Quantity+�� 5?      $��Style: ��6(   $
��Bold"�67   
�������6I   �� 5@      ��6(   �� *Style+5A      5B      5C      5E      �� 5H   
   -   5        $��Update Data��5H      $��Right=�5I      $��Bottom=�5J      $��10q�5K      $��10q�5L      5M      5N      5O      