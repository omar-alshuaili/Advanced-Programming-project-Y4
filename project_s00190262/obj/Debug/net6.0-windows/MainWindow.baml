   M S B A M L   `   `   `  ����L  Hproject_s00190262, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:project_s00190262project_s00190262  5   	   "    project_s00190262.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:project_s00190262     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5         ��Name$ buttonSelectFiles��5      $��Select Files��6,   $
��Left=�6C   $��10q�6^   $	��Top=�6j   $	��120��6�   6�   �� 5   
   -   5        $ buttonStartCheck��5      $��Start Check��6+   $��1��6A   $
��Left=�6N   $��10q�6i   $	��Top=�6u   $	��120��6�   6�   �� 5   
   -   5        $ buttonReplaceWords��5      $��Replace Words��6-   $��1��6E   $��1��6R   $��Right=�6b   $��0,0,10,0q�6~   $	��Top=�6�   $	��120��6�   6�   �� 5   
   -   5         ��Name$ listBoxMisspelledWords��5      $��2��62   $��0��6?   $��10q�6O   �� 5   
   -   5        $ listBoxCorrectSpelling��5      $��2��62   $��1��6?   $��10q�6O   �� 5   
   $��1��6   $��2��6"    �� Orientation$ 
Horizontal=�62   $��Right=�6K   $	��Top=�6g   $��10q�6   .+�5       	� -   5         	�Name$ radioButtonDark��5       $
��Dark��63   6B   	� 5!      -   5        $ radioButtonLight��5!      $��Light��64   6D   5#      5%      5&      