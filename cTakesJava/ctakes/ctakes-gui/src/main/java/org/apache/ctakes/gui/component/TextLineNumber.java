package org.apache.ctakes.gui.component;


import javax.swing.*;
import javax.swing.border.Border;
import javax.swing.border.CompoundBorder;
import javax.swing.border.EmptyBorder;
import javax.swing.border.MatteBorder;
import javax.swing.event.CaretEvent;
import javax.swing.event.CaretListener;
import javax.swing.event.DocumentEvent;
import javax.swing.event.DocumentListener;
import javax.swing.text.*;
import java.awt.*;
import java.awt.geom.Rectangle2D;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.util.HashMap;
import java.util.Map;

/**
 * @author SPF , chip-nlp
 * @version %I%
 * @since 4/5/2017
 */
public class TextLineNumber extends JPanel
      implements CaretListener, DocumentListener, PropertyChangeListener {

   public final static float LEFT = 0.0f;
   public final static float CENTER = 0.5f;
   public final static float RIGHT = 1.0f;

   private final static Border OUTER = new MatteBorder( 0, 0, 0, 2, Color.GRAY );

   private final static int HEIGHT = Integer.MAX_VALUE - 1000000;

   //  Text component this TextTextLineNumber component is in sync with

   private final JTextComponent _component;

   //  Properties that can be changed

   private boolean _updateFont;
   private int _borderGap;
   private Color _currentLineForeground;
   private float _digitAlignment;
   private int _minimumDisplayDigits;

   //  Keep history information to reduce the number of times the component
   //  needs to be repainted

   private int _lastDigits;
   private double _lastHeight;
   private int _lastLine;

   private Map<String, FontMetrics> _fonts;

   /**
    * Create a line number component for a text component. This minimum
    * display width will be based on 3 digits.
    *
    * @param component the related text component
    */
   public TextLineNumber( JTextComponent component ) {
      this( component, 3 );
   }

   /**
    * Create a line number component for a text component.
    *
    * @param component            the related text component
    * @param minimumDisplayDigits the number of digits used to calculate
    *                             the minimum width of the component
    */
   public TextLineNumber( JTextComponent component, int minimumDisplayDigits ) {
      _component = component;

      setFont( component.getFont() );
      setForeground( Color.DARK_GRAY );

      setBorderGap( 5 );
      setCurrentLineForeground( Color.MAGENTA );
      setDigitAlignment( RIGHT );
      setMinimumDisplayDigits( minimumDisplayDigits );

      component.getDocument().addDocumentListener( this );
      component.addCaretListener( this );
      component.addPropertyChangeListener( "font", this );
   }

   /**
    * Gets the update font property
    *
    * @return the update font property
    */
   public boolean getUpdateFont() {
      return _updateFont;
   }

   /**
    * Set the update font property. Indicates whether this Font should be
    * updated automatically when the Font of the related text component
    * is changed.
    *
    * @param updateFont when true update the Font and repaint the line
    *                   numbers, otherwise just repaint the line numbers.
    */
   public void setUpdateFont( boolean updateFont ) {
      _updateFont = updateFont;
   }

   /**
    * Gets the border gap
    *
    * @return the border gap in pixels
    */
   public int getBorderGap() {
      return _borderGap;
   }

   /**
    * The border gap is used in calculating the left and right insets of the
    * border. Default value is 5.
    *
    * @param borderGap the gap in pixels
    */
   public void setBorderGap( int borderGap ) {
      _borderGap = borderGap;
      Border inner = new EmptyBorder( 0, borderGap, 0, borderGap );
      setBorder( new CompoundBorder( OUTER, inner ) );
      _lastDigits = 0;
      setPreferredWidth();
   }

   /**
    * Gets the current line rendering Color
    *
    * @return the Color used to render the current line number
    */
   public Color getCurrentLineForeground() {
      return _currentLineForeground == null ? getForeground() : _currentLineForeground;
   }

   /**
    * The Color used to render the current line digits. Default is Coolor.RED.
    *
    * @param currentLineForeground the Color used to render the current line
    */
   public void setCurrentLineForeground( Color currentLineForeground ) {
      _currentLineForeground = currentLineForeground;
   }

   /**
    * Gets the digit alignment
    *
    * @return the alignment of the painted digits
    */
   public float getDigitAlignment() {
      return _digitAlignment;
   }

   /**
    * Specify the horizontal alignment of the digits within the component.
    * Common values would be:
    * <ul>
    * <li>TextLineNumber.LEFT
    * <li>TextLineNumber.CENTER
    * <li>TextLineNumber.RIGHT (default)
    * </ul>
    *
    * @param digitAlignment LEFT, CENTER, of RIGHT
    */
   public void setDigitAlignment( float digitAlignment ) {
      _digitAlignment =
            digitAlignment > 1.0f ? 1.0f : digitAlignment < 0.0f ? -1.0f : digitAlignment;
   }

   /**
    * Gets the minimum display digits
    *
    * @return the minimum display digits
    */
   public int getMinimumDisplayDigits() {
      return _minimumDisplayDigits;
   }

   /**
    * Specify the mimimum number of digits used to calculate the preferred
    * width of the component. Default is 3.
    *
    * @param minimumDisplayDigits the number digits used in the preferred
    *                             width calculation
    */
   public void setMinimumDisplayDigits( int minimumDisplayDigits ) {
      _minimumDisplayDigits = minimumDisplayDigits;
      setPreferredWidth();
   }

   /**
    * Calculate the width needed to display the maximum line number
    */
   private void setPreferredWidth() {
      Element root = _component.getDocument().getDefaultRootElement();
      int lines = root.getElementCount();
      int digits = Math.max( String.valueOf( lines ).length(), _minimumDisplayDigits );

      //  Update sizes when number of digits in the line number changes

      if ( _lastDigits != digits ) {
         _lastDigits = digits;
         FontMetrics fontMetrics = getFontMetrics( getFont() );
         int width = fontMetrics.charWidth( '0' ) * digits;
         Insets insets = getInsets();
         int preferredWidth = insets.left + insets.right + width;

         Dimension d = getPreferredSize();
         d.setSize( preferredWidth, HEIGHT );
         setPreferredSize( d );
         setSize( d );
      }
   }

   /**
    * Draw the line numbers
    */
   @Override
   public void paintComponent( Graphics g ) {
      super.paintComponent( g );

      //	Determine the width of the space available to draw the line number

      FontMetrics fontMetrics = _component.getFontMetrics( _component.getFont() );
      Insets insets = getInsets();
      int availableWidth = getSize().width - insets.left - insets.right;

      //  Determine the rows to draw within the clipped bounds.

      Rectangle clip = g.getClipBounds();
      int rowStartOffset = _component.viewToModel2D( new Point( 0, clip.y ) );
      int endOffset = _component.viewToModel2D( new Point( 0, clip.y + clip.height ) );

      while ( rowStartOffset <= endOffset ) {
         try {
            if ( isCurrentLine( rowStartOffset ) ) {
               g.setColor( getCurrentLineForeground() );
            } else {
               g.setColor( getForeground() );
            }

            //  Get the line number as a string and then determine the
            //  "X" and "Y" offsets for drawing the string.

            String lineNumber = getTextLineNumber( rowStartOffset );
            int stringWidth = fontMetrics.stringWidth( lineNumber );
            int x = getOffsetX( availableWidth, stringWidth ) + insets.left;
            int y = getOffsetY( rowStartOffset, fontMetrics );
            g.drawString( lineNumber, x, y );

            //  Move to the next row

            rowStartOffset = Utilities.getRowEnd( _component, rowStartOffset ) + 1;
         } catch ( Exception e ) {
            break;
         }
      }
   }

   /*
    *  We need to know if the caret is currently positioned on the line we
    *  are about to paint so the line number can be highlighted.
    */
   private boolean isCurrentLine( int rowStartOffset ) {
      int caretPosition = _component.getCaretPosition();
      Element root = _component.getDocument().getDefaultRootElement();

      if ( root.getElementIndex( rowStartOffset ) == root.getElementIndex( caretPosition ) ) {
         return true;
      } else {
         return false;
      }
   }

   /*
    *	Get the line number to be drawn. The empty string will be returned
    *  when a line of text has wrapped.
    */
   protected String getTextLineNumber( int rowStartOffset ) {
      Element root = _component.getDocument().getDefaultRootElement();
      int index = root.getElementIndex( rowStartOffset );
      Element line = root.getElement( index );

      if ( line.getStartOffset() == rowStartOffset ) {
         return String.valueOf( index + 1 );
      } else {
         return "";
      }
   }

   /*
    *  Determine the X offset to properly align the line number when drawn
    */
   private int getOffsetX( int availableWidth, int stringWidth ) {
      return (int)((availableWidth - stringWidth) * _digitAlignment);
   }

   /*
    *  Determine the Y offset for the current row
    */
   private int getOffsetY( int rowStartOffset, FontMetrics fontMetrics )
         throws BadLocationException {
      //  Get the bounding rectangle of the row

      Rectangle2D r = _component.modelToView2D( rowStartOffset );
      int lineHeight = fontMetrics.getHeight();
      int y = Double.valueOf( r.getY() + r.getHeight() ).intValue();
      int descent = 0;

      //  The text needs to be positioned above the bottom of the bounding
      //  rectangle based on the descent of the font(s) contained on the row.

      if ( r.getHeight() == lineHeight )  // default font is being used
      {
         descent = fontMetrics.getDescent();
      } else  // We need to check all the attributes for font changes
      {
         if ( _fonts == null ) {
            _fonts = new HashMap<String, FontMetrics>();
         }

         Element root = _component.getDocument().getDefaultRootElement();
         int index = root.getElementIndex( rowStartOffset );
         Element line = root.getElement( index );

         for ( int i = 0; i < line.getElementCount(); i++ ) {
            Element child = line.getElement( i );
            AttributeSet as = child.getAttributes();
            String fontFamily = (String)as.getAttribute( StyleConstants.FontFamily );
            Integer fontSize = (Integer)as.getAttribute( StyleConstants.FontSize );
            String key = fontFamily + fontSize;

            FontMetrics fm = _fonts.get( key );

            if ( fm == null ) {
               Font font = new Font( fontFamily, Font.PLAIN, fontSize );
               fm = _component.getFontMetrics( font );
               _fonts.put( key, fm );
            }

            descent = Math.max( descent, fm.getDescent() );
         }
      }

      return y - descent;
   }

   //
//  Implement CaretListener interface
//
   @Override
   public void caretUpdate( CaretEvent e ) {
      //  Get the line the caret is positioned on

      int caretPosition = _component.getCaretPosition();
      Element root = _component.getDocument().getDefaultRootElement();
      int currentLine = root.getElementIndex( caretPosition );

      //  Need to repaint so the correct line number can be highlighted

      if ( _lastLine != currentLine ) {
         repaint();
         _lastLine = currentLine;
      }
   }

   //
//  Implement DocumentListener interface
//
   @Override
   public void changedUpdate( DocumentEvent e ) {
      documentChanged();
   }

   @Override
   public void insertUpdate( DocumentEvent e ) {
      documentChanged();
   }

   @Override
   public void removeUpdate( DocumentEvent e ) {
      documentChanged();
   }

   /*
    *  A document change may affect the number of displayed lines of text.
    *  Therefore the lines numbers will also change.
    */
   private void documentChanged() {
      //  View of the component has not been updated at the time
      //  the DocumentEvent is fired

      SwingUtilities.invokeLater( () -> {
         try {
            int endPos = _component.getDocument().getLength();
            Rectangle2D rect = _component.modelToView2D( endPos );

            if ( rect != null && rect.getY() != _lastHeight ) {
               setPreferredWidth();
               repaint();
               _lastHeight = rect.getY();
            }
         } catch ( BadLocationException ex ) { /* nothing to do */ }
      } );
   }

   //
//  Implement PropertyChangeListener interface
//
   @Override
   public void propertyChange( PropertyChangeEvent evt ) {
      if ( evt.getNewValue() instanceof Font ) {
         if ( _updateFont ) {
            Font newFont = (Font)evt.getNewValue();
            setFont( newFont );
            _lastDigits = 0;
            setPreferredWidth();
         } else {
            repaint();
         }
      }
   }


}
