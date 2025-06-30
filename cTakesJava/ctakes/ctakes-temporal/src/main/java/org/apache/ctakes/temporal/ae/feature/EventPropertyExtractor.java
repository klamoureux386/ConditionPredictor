/**
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
package org.apache.ctakes.temporal.ae.feature;

import org.apache.ctakes.core.util.doc.DocIdUtil;
import org.apache.ctakes.typesystem.type.syntax.BaseToken;
import org.apache.ctakes.typesystem.type.syntax.NewlineToken;
import org.apache.ctakes.typesystem.type.syntax.WordToken;
import org.apache.ctakes.typesystem.type.textsem.EventMention;
import org.apache.ctakes.typesystem.type.textspan.Sentence;
import org.apache.uima.fit.util.JCasUtil;
import org.apache.uima.jcas.JCas;
import org.apache.uima.jcas.tcas.Annotation;
import org.cleartk.ml.Feature;
import org.cleartk.ml.feature.extractor.CleartkExtractorException;
import org.cleartk.ml.feature.extractor.FeatureExtractor1;

import java.util.*;

//import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class EventPropertyExtractor implements FeatureExtractor1<Annotation> {

	//	private String name;
	private static Integer polarity;

	//  private Logger LOGGER = LoggerFactory.getLogger(this.getClass().getName());

	public EventPropertyExtractor() {
		super();
		//		this.name = "EventContextualModality";

	}
	
	private static final List<String> genericWords = new ArrayList<>();
	static{
		genericWords.add("potential");
		genericWords.add("possible");
		genericWords.add("may");
		genericWords.add("likely");
		genericWords.add("probable");			
		genericWords.add("prospective");
		genericWords.add("instruct");
		genericWords.add("if");//newly added 4 on July 13 2016
		genericWords.add("could");
		genericWords.add("discussed");
		genericWords.add("discussion");
		genericWords.add("considered");
		genericWords.add("monitor");//newly added on Aug 19 2016
		genericWords.add("plan");//newly added on Aug 19 2016
		genericWords.add("cxr");
		genericWords.add("data");
		//			genericWords.add("change");
		//			genericWords.add("prescription");
		//			genericWords.add("prescribe");
		//			genericWords.add("prescribed");
		//			genericWords.add("speak");
		//			genericWords.add("spoke");
	}

	@Override
	public List<Feature> extract(JCas view, Annotation annotation) throws CleartkExtractorException {
		List<Feature> features = new ArrayList<>();
		
		//get Document ID:
		String fname;
		try {
         String docID = DocIdUtil.getDocumentID( view );//ViewUriUtil.getURI(view).toString();
			
			int begin = docID.lastIndexOf("_");
			fname = docID.substring(begin+1);
			features.add(new Feature("docName", fname));
			
			if(fname.equals("RAD")||fname.equals("SP")){
				features.add(new Feature("docName:RAD+SP"));
			}else{
				features.add(new Feature("docName:others"));
			}
			
//		} catch (AnalysisEngineProcessException e) {
//			e.printStackTrace();
//			fname = "AnalysisEngineProcessException.UnableToGetDocIdFromUriView";
//			features.add(new Feature("docName", fname));
			
		} catch (org.apache.uima.cas.CASRuntimeException e) { // for unit tests that don't set up the UriView 
			e.printStackTrace();
			fname = "CASRuntimeException.UnableToGetDocIdFromUriView";
			features.add(new Feature("docName", fname));
		}

		//1 get event:
		EventMention event = (EventMention)annotation;
		//		if(event.getEvent()!= null && event.getEvent().getProperties() != null){
		//			String contextModal = event.getEvent().getProperties().getContextualModality();
		//			if ( "GENERIC".equals(contextModal) ){
		//				Feature contexmod = new Feature(this.name, contextModal);
		//				features.add(contexmod);
		//				//		  LOGGER.info("found a event: "+ contextModal);
		//			}
		//		}

		Set<Sentence> coveringSents = new HashSet<>();
		coveringSents.addAll(JCasUtil.selectCovering(view, Sentence.class, event.getBegin(), event.getEnd()));


		for(Sentence coveringSent : coveringSents){
			List<EventMention> events = JCasUtil.selectCovered(EventMention.class, coveringSent);
			List<EventMention> realEvents = new ArrayList<>();
			for(EventMention eventa : events){
				// filter out ctakes events
				if(eventa.getClass().equals(EventMention.class)){
					realEvents.add(eventa);
				}
			}
			events = realEvents;
			if( events.size()>0){
				EventMention anchor = events.get(0);
				EventMention end    = events.get(events.size()-1);
				if(event == anchor){
					features.add(new Feature("LeftMostEvent"));
				}else if( event == end){
					features.add(new Feature("RightMostEvent"));
				}
			}
			
			//check if this event is generic:
			List<WordToken> words = new ArrayList<>(JCasUtil.selectPreceding(view, WordToken.class, event, 15));
			words.addAll(JCasUtil.selectFollowing(view, WordToken.class, event, 15));
			for(WordToken word : words){
				if(outsideScope(word, coveringSent)){//if the word is outside the sentence
					continue;
				}
				if(genericWords.contains(word.getCoveredText().toLowerCase())){
					features.add(new Feature("GenericEvent"));
					break;
				}
			}
			
			//check how many words are in the event mention:
//			List<WordToken> coveredWords = new ArrayList<>(JCasUtil.selectCovered(view, WordToken.class, event));
//			int numWords = coveredWords.size();
//			if(numWords==1){
//				features.add(new Feature("singleWordEvent"));
//			}
//			features.add(new Feature("Event_Word_num", numWords));
			
			//check if there is any newLine token in close vicinity:
			int newlineNum = 0;
			for (BaseToken btoken: JCasUtil.selectPreceding(view, BaseToken.class, event, 20)){
				if(btoken instanceof NewlineToken){
					newlineNum++;
				}
			}
			if(newlineNum > 0){
				features.add(new Feature("hasPrecedingNewline"));
				features.add(new Feature("newLineNum_preceding", newlineNum));
			}
			newlineNum = 0;
			for (BaseToken btoken: JCasUtil.selectFollowing(view, BaseToken.class, event, 20)){
				if(btoken instanceof NewlineToken){
					newlineNum++;
				}
			}
			if(newlineNum > 0){
				features.add(new Feature("hasFollowingNewline"));
				features.add(new Feature("newLineNum_following", newlineNum));
			}
			
			//check if there is any semi-column is close vicinity:
//			int	semiColumnNum = 0;
//			for (BaseToken btoken: JCasUtil.selectFollowing(view, BaseToken.class, event, 5)){
//				if(btoken instanceof PunctuationToken){
//					if(btoken.getCoveredText().equals(":")){
//						semiColumnNum++;
//					}
//				}
//			}
//			if(semiColumnNum > 0){
//				features.add(new Feature("hasFollowingSemiColumn"));
//				features.add(new Feature("semiColumn_following", semiColumnNum));
//			}
		}

		features.addAll(getEventFeats("mentionProperty", event));

		return features;
	}
	
	private static boolean outsideScope(WordToken word, Sentence eventSent) {
		if(word.getBegin()< eventSent.getBegin()){
			return true;
		}else if(word.getEnd()>eventSent.getEnd()){
			return true;
		}
		return false;
	}


	private static Collection<? extends Feature> getEventFeats(String name, EventMention mention) {
		List<Feature> feats = new ArrayList<>();
		//add contextual modality as a feature
		if(mention.getEvent()==null || mention.getEvent().getProperties() == null){
			return feats;
		}
		String contextualModality = mention.getEvent().getProperties().getContextualModality();
		if (contextualModality != null)
			feats.add(new Feature(name + "_modality", contextualModality));

		polarity = mention.getEvent().getProperties().getPolarity();
		if(polarity!=null )
			feats.add(new Feature(name + "_polarity", polarity));
		//    feats.add(new Feature(name + "_category", mention.getEvent().getProperties().getCategory()));//null
		//    feats.add(new Feature(name + "_degree", mention.getEvent().getProperties().getDegree()));//null
		
		return feats;
	}


	public List<Feature> extract( final JCas view, final Annotation annotation,
											final Collection<EventMention> events,
											final List<BaseToken> sortedTokens,
											final List<WordToken> sortedWords ) throws CleartkExtractorException {
		final List<Feature> features = new ArrayList<>();
		//get Document ID:
		String fname;
		try {
			final String docID = DocIdUtil.getDocumentID( view );//ViewUriUtil.getURI(view).toString();
			int begin = docID.lastIndexOf( "_" );
			fname = docID.substring( begin+1 );
			features.add( new Feature( "docName", fname ) );
			if ( fname.equals( "RAD" ) || fname.equals( "SP" ) ) {
				features.add( new Feature( "docName:RAD+SP" ) );
			}else{
				features.add( new Feature( "docName:others" ) );
			}
		} catch ( org.apache.uima.cas.CASRuntimeException casRTE ) { // for unit tests that don't set up the UriView
			casRTE.printStackTrace();
			features.add( new Feature( "docName", "CASRuntimeException.UnableToGetDocIdFromUriView" ) );
		}

		//1 get event:
		final EventMention event = (EventMention)annotation;
		final List<EventMention> realEvents = new ArrayList<>();
		for ( EventMention eventa : events ) {
			// filter out umls events  --> I am not sure that this is good.  Perhaps EventMention.getEvent() == null ?
			if( eventa.getClass().equals( EventMention.class ) ) {
				realEvents.add( eventa );
			}
		}
		if ( realEvents.size() > 0 ) {
			if ( event.equals( realEvents.get( 0 ) ) ) {
				// Event is anchor, first "real event"
				features.add( new Feature( "LeftMostEvent" ) );
			} else if ( event.equals( realEvents.get(realEvents.size()-1) ) ) {
				// event is end, last "real event"
				features.add( new Feature( "RightMostEvent" ) );
			}
		}

		//check if this event is generic:
		final int eventBegin = event.getBegin();
		int wordIndex = 0;
		for ( int i=0; i<sortedWords.size(); i++ ) {
			if ( sortedWords.get( i ).getBegin() >= eventBegin ) {
				wordIndex = i;
				break;
			}
		}
		final int lowLimit = Math.max( 0, wordIndex-15 );
		final int highLimit = Math.min( sortedWords.size(), wordIndex + 16 );
		for ( int i=lowLimit; i<highLimit; i++ ) {
			if ( i == wordIndex ) {
				continue;
			}
			if ( genericWords.contains( sortedWords.get( i ).getCoveredText().toLowerCase() ) ) {
				features.add( new Feature( "GenericEvent" ) );
				break;
			}
		}
		int tokenIndex = 0;
		for ( int i=0; i<sortedTokens.size(); i++ ) {
			if ( sortedTokens.get( i ).getBegin() >= eventBegin ) {
				tokenIndex = i;
				break;
			}
		}
		int crLimit = Math.max( 0, tokenIndex-20 );
		//check if there is any newLine token in close vicinity:
		int newlineNum = 0;
		for ( int i=crLimit; i<tokenIndex; i++ ) {
			if ( sortedTokens.get( i ) instanceof NewlineToken ) {
				newlineNum++;
			}
		}
		if ( newlineNum > 0 ) {
			features.add( new Feature( "hasPrecedingNewline" ) );
			features.add( new Feature( "newLineNum_preceding", newlineNum ) );
		}
		crLimit = Math.min( sortedTokens.size(), tokenIndex + 21 );
		newlineNum = 0;
		for ( int i=tokenIndex+1; i<crLimit; i++ ) {
			if ( sortedTokens.get( i ) instanceof NewlineToken ) {
				newlineNum++;
			}
		}
		if ( newlineNum > 0 ) {
			features.add( new Feature( "hasFollowingNewline" ) );
			features.add( new Feature( "newLineNum_following", newlineNum ) );
		}
		features.addAll( getEventFeats( "mentionProperty", event ) );
		return features;
	}




}
