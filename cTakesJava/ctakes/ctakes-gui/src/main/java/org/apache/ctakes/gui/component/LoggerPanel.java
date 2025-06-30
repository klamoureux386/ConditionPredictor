package org.apache.ctakes.gui.component;


import org.apache.ctakes.log.ObservableLogger;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.event.Level;
import org.slf4j.event.LoggingEvent;
import org.slf4j.spi.LoggingEventAware;

import javax.swing.*;
import javax.swing.text.BadLocationException;
import javax.swing.text.DefaultStyledDocument;
import java.awt.*;
import java.util.Arrays;
import java.util.Collection;

/**
 * @author SPF , chip-nlp
 * @version %I%
 * @since 11/29/2016
 */
final public class LoggerPanel extends JScrollPane {

   static private final int MAX_DOC_LENGTH = Integer.MAX_VALUE - 5000;

   static public LoggerPanel createLoggerPanel( final Level... levels ) {
      final LoggerPanel panel = new LoggerPanel( levels );
      final Logger rootLogger = LoggerFactory.getLogger( org.slf4j.Logger.ROOT_LOGGER_NAME );
      addLogListener( rootLogger, panel.getLogHandler() );
      return panel;
   }

   static private void addLogListener( final Logger logger, final LoggingEventAware listener ) {
      if ( logger instanceof ObservableLogger ) {
         ((ObservableLogger)logger).addListener( listener );
      } else {
         logger.error( logger.getClass().getName() + " is not a org.apache.ctakes.log.ObservableLogger.  Cannot display logs in GUI." );
      }
   }

   static private final Level[] ALL_LEVELS = { Level.ERROR, Level.WARN, Level.INFO, Level.DEBUG, Level.TRACE };


   private final LogHandler _logHandler;
   private final DefaultStyledDocument _textAreaDoc = new DefaultStyledDocument();

   /**
    * text gui that will display log4j messages
    */
   private LoggerPanel( final Level... levels ) {
      new LoggerTextFilter( _textAreaDoc );
      final JTextPane textPane = new JTextPane( _textAreaDoc );
      textPane.setFont( new Font( Font.MONOSPACED, Font.PLAIN, 14 ) );
      textPane.setEditable( false );
      super.setViewportView( textPane );
      _logHandler = new LogHandler( levels );
   }

   /**
    * @return all the text in this gui
    */
   public String getText() {
      try {
         return _textAreaDoc.getText( 0, _textAreaDoc.getLength() );
      } catch ( BadLocationException blE ) {
         return "";
      }
   }

   /**
    * clear the text in this gui
    */
   public void clearText() {
      SwingUtilities.invokeLater( () -> {
         try {
            _textAreaDoc.remove( 0, _textAreaDoc.getLength() );
         } catch ( BadLocationException blE ) {
            //
         }
      } );
   }

   /**
    * @param text to append to the text displayed in this gui
    */
   public void appendText( final String text ) {
      SwingUtilities.invokeLater( () -> {
         try {
            if (_textAreaDoc.getLength() >= MAX_DOC_LENGTH ) {
               // clear log if it gets too long.  Not the best solution, but good enough for now.
               _textAreaDoc.remove( 0, _textAreaDoc.getLength() );
            }
            _textAreaDoc.insertString( _textAreaDoc.getLength(), text, null );
         } catch ( BadLocationException blE ) {
            //
         }
      } );
   }


   /**
    * @return the log4j appender that handles logging
    */
   private LoggingEventAware getLogHandler() {
      return _logHandler;
   }

   /**
    * Handles reception of logging messages
    */
   private class LogHandler implements LoggingEventAware {
      private final Collection<Level> _levels;
      private LogHandler( final Level... levels ) {
         _levels = Arrays.asList( (levels.length == 0 ? ALL_LEVELS : levels) );
      }

      /**
       * {@inheritDoc}
       */
      @Override
      public void log( final LoggingEvent event ) {
         if ( event == null ) {
            return;
         }
         final Level level = event.getLevel();
         if ( _levels.contains( level ) ) {
            final String message = constructMessage( event );
            if ( message != null ) {
               String text = message;
               if ( text.equals( "." ) ) {
                  appendText( text );
                  return;
               }
               if ( level.equals( Level.ERROR ) ) {
                  if ( !text.toLowerCase()
                            .contains( "error" ) ) {
                     text = "Error: " + text;
                  }
               } else if ( level.equals( Level.WARN ) ) {
                  if ( !text.toLowerCase()
                            .contains( "warn" ) ) {
                     text = "Warning: " + text;
                  }
               } else if ( level.equals( Level.DEBUG ) ) {
                  text = "// " + text;
               }
               appendText( text + "\n" );
            }
         }
      }

      /**
       *
       * @param event slf4j event
       * @return message reconstructed from parameterized logging: ( "text {}", arg )
       */
      static private String constructMessage( final LoggingEvent event ) {
         return constructMessage( event.getMessage(), event.getArguments() );
      }

      /**
       *
       * @param message -
       * @param args -
       * @return message reconstructed from parameterized logging: ( "text {}", arg )
       */
      static private String constructMessage( final String message, final java.util.List<Object> args ) {
         if ( args == null || args.isEmpty() ) {
            return message;
         }
         final StringBuilder sb = new StringBuilder();
         int lastIndex = 0;
         for ( Object arg : args ) {
            final int nextIndex = message.indexOf( "{}", lastIndex );
            if ( nextIndex < 0 ) {
               break;
            }
            if ( nextIndex > lastIndex ) {
               sb.append( message, lastIndex, nextIndex );
            }
            sb.append( arg.toString() );
            lastIndex = nextIndex+2;
         }
         if ( lastIndex < message.length() ) {
            sb.append( message.substring( lastIndex ) );
         }
         return sb.toString();
      }

//      /**
//       * {@inheritDoc}
//       *
//       * @return false
//       */
//      @Override
//      public boolean requiresLayout() {
//         return false;
//      }
//
//      /**
//       * {@inheritDoc}
//       */
//      @Override
//      public void close() {
//      }
   }

//   public static void main( String[] args ) {
//      System.out.println( "EMPTY:" + LogHandler.constructMessage( "", null ));
//      System.out.println( "ONE:" + LogHandler.constructMessage( "{}", Collections.singletonList("Hello") ));
//      System.out.println( "TWO:" + LogHandler.constructMessage( "{}{}", Arrays.asList( "One","Two" ) ));
//      System.out.println( "TWO1:" + LogHandler.constructMessage( " {}{}", Arrays.asList( "One","Two" ) ));
//      System.out.println( "TWO2:" + LogHandler.constructMessage( "{} {}", Arrays.asList( "One","Two" ) ));
//      System.out.println( "TWO3:" + LogHandler.constructMessage( "{}{} ", Arrays.asList( "One","Two" ) ));
//      System.out.println( "TWO1:" + LogHandler.constructMessage( "- {}{}-", Arrays.asList( "One","Two" ) ));
//      System.out.println( "TWO2:" + LogHandler.constructMessage( "-{} -{}", Arrays.asList( "One","Two" ) ));
//      System.out.println( "TWO3:" + LogHandler.constructMessage( "-{}{}- -", Arrays.asList( "One","Two" ) ));
//   }

}
