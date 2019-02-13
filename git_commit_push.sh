

Msg=$1

git add .

if [ "$Msg" == "" ]
then
    git commit -m "commit and push" 
else 
    git commit -m "$Msg"
fi

git push
